using NexoFleet.Domain.Common;
using NexoFleet.Domain.Payments.Events;

namespace NexoFleet.Domain.Payments;

public sealed class PaymentReport : AggregateRoot
{
    private readonly List<PaymentItem> _items = [];
    private readonly List<PaymentComment> _comments = [];
    private readonly List<PaymentReportFile> _files = [];

    private PaymentReport(
        Guid id,
        Guid companyId,
        Guid paymentPeriodId,
        Guid employeeId,
        decimal baseAmount,
        string currency,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        PaymentPeriodId = paymentPeriodId;
        EmployeeId = employeeId;
        BaseAmount = baseAmount;
        Currency = currency;
        Status = PaymentReportStatus.Draft;
        CreatedAtUtc = createdAtUtc;
    }

    private PaymentReport() { }

    public Guid CompanyId { get; private set; }
    public Guid PaymentPeriodId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal BaseAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public PaymentReportStatus Status { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public string? VoidedReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<PaymentItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<PaymentComment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<PaymentReportFile> Files => _files.AsReadOnly();
    public decimal Additions => _items.Where(item => item.Effect == PaymentItemEffect.Addition).Sum(item => item.Amount);
    public decimal Deductions => _items.Where(item => item.Effect == PaymentItemEffect.Deduction).Sum(item => item.Amount);
    public decimal TotalAmount => BaseAmount + Additions - Deductions;

    public static Result<PaymentReport> Create(
        Guid id,
        Guid companyId,
        Guid paymentPeriodId,
        Guid employeeId,
        decimal baseAmount,
        string currency,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty) return Result<PaymentReport>.Failure(PaymentErrors.InvalidId);
        if (companyId == Guid.Empty) return Result<PaymentReport>.Failure(PaymentErrors.InvalidCompanyId);
        if (paymentPeriodId == Guid.Empty) return Result<PaymentReport>.Failure(PaymentErrors.InvalidPeriodId);
        if (employeeId == Guid.Empty) return Result<PaymentReport>.Failure(PaymentErrors.InvalidEmployeeId);
        var amountResult = ValidateAmountAndCurrency(baseAmount, currency);
        if (amountResult.IsFailure) return Result<PaymentReport>.Failure(amountResult.Error);

        return Result<PaymentReport>.Success(new PaymentReport(
            id, companyId, paymentPeriodId, employeeId, baseAmount, NormalizeCurrency(currency), createdAtUtc));
    }

    public Result UpdateBaseAmount(decimal baseAmount, string currency, DateTimeOffset updatedAtUtc)
    {
        var draftResult = EnsureDraft();
        if (draftResult.IsFailure) return draftResult;
        var validation = ValidateAmountAndCurrency(baseAmount, currency);
        if (validation.IsFailure) return validation;

        var normalizedCurrency = NormalizeCurrency(currency);
        if (BaseAmount == baseAmount && Currency == normalizedCurrency) return Result.Success();
        BaseAmount = baseAmount;
        Currency = normalizedCurrency;
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public Result AddItem(
        Guid itemId,
        Guid? tripId,
        PaymentItemEffect effect,
        string description,
        decimal amount,
        DateTimeOffset createdAtUtc)
    {
        var draftResult = EnsureDraft();
        if (draftResult.IsFailure) return draftResult;
        var validation = ValidateItem(itemId, tripId, effect, description, amount);
        if (validation.IsFailure) return validation;
        if (_items.Any(item => item.Id == itemId)) return Result.Failure(PaymentErrors.ItemAlreadyExists);

        _items.Add(new PaymentItem(itemId, Id, CompanyId, tripId, effect, Normalize(description), amount, createdAtUtc));
        UpdatedAtUtc = createdAtUtc;
        return Result.Success();
    }

    public Result UpdateItem(Guid itemId, PaymentItemEffect effect, string description, decimal amount, DateTimeOffset updatedAtUtc)
    {
        var draftResult = EnsureDraft();
        if (draftResult.IsFailure) return draftResult;
        var item = _items.SingleOrDefault(candidate => candidate.Id == itemId);
        if (item is null) return Result.Failure(PaymentErrors.ItemNotFound);
        var validation = ValidateItem(itemId, item.TripId, effect, description, amount);
        if (validation.IsFailure) return validation;

        var normalizedDescription = Normalize(description);
        if (item.Effect == effect && item.Description == normalizedDescription && item.Amount == amount) return Result.Success();
        item.Update(effect, normalizedDescription, amount, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public Result RemoveItem(Guid itemId, DateTimeOffset updatedAtUtc)
    {
        var draftResult = EnsureDraft();
        if (draftResult.IsFailure) return draftResult;
        var item = _items.SingleOrDefault(candidate => candidate.Id == itemId);
        if (item is null) return Result.Failure(PaymentErrors.ItemNotFound);
        _items.Remove(item);
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public Result AddComment(Guid commentId, Guid authorUserId, string text, DateTimeOffset createdAtUtc)
    {
        if (Status == PaymentReportStatus.Voided) return Result.Failure(PaymentErrors.VoidedStatusIsFinal);
        if (commentId == Guid.Empty) return Result.Failure(PaymentErrors.InvalidId);
        if (authorUserId == Guid.Empty) return Result.Failure(PaymentErrors.InvalidUserId);
        if (string.IsNullOrWhiteSpace(text)) return Result.Failure(PaymentErrors.CommentRequired);
        if (text.Trim().Length > PaymentErrors.CommentMaxLength) return Result.Failure(PaymentErrors.CommentTooLong);
        if (_comments.Any(comment => comment.Id == commentId)) return Result.Failure(PaymentErrors.CommentAlreadyExists);
        _comments.Add(new PaymentComment(commentId, Id, CompanyId, authorUserId, Normalize(text), createdAtUtc));
        UpdatedAtUtc = createdAtUtc;
        return Result.Success();
    }

    public Result AddFile(
        Guid fileId,
        string fileName,
        string storageKey,
        string contentType,
        long sizeInBytes,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAtUtc)
    {
        var draftResult = EnsureDraft();
        if (draftResult.IsFailure) return draftResult;
        if (fileId == Guid.Empty) return Result.Failure(PaymentErrors.InvalidId);
        if (uploadedByUserId == Guid.Empty) return Result.Failure(PaymentErrors.InvalidUserId);
        if (string.IsNullOrWhiteSpace(fileName)) return Result.Failure(PaymentErrors.FileNameRequired);
        if (string.IsNullOrWhiteSpace(storageKey)) return Result.Failure(PaymentErrors.StorageKeyRequired);
        if (string.IsNullOrWhiteSpace(contentType)) return Result.Failure(PaymentErrors.ContentTypeRequired);
        if (sizeInBytes <= 0) return Result.Failure(PaymentErrors.InvalidFileSize);
        if (fileName.Trim().Length > PaymentErrors.FileNameMaxLength || storageKey.Trim().Length > PaymentErrors.StorageKeyMaxLength || contentType.Trim().Length > PaymentErrors.ContentTypeMaxLength)
            return Result.Failure(PaymentErrors.FileMetadataTooLong);
        if (_files.Any(file => file.Id == fileId)) return Result.Failure(PaymentErrors.FileAlreadyExists);

        _files.Add(new PaymentReportFile(fileId, Id, CompanyId, Normalize(fileName), Normalize(storageKey), Normalize(contentType).ToLowerInvariant(), sizeInBytes, uploadedByUserId, uploadedAtUtc));
        UpdatedAtUtc = uploadedAtUtc;
        return Result.Success();
    }

    public Result Publish(DateTimeOffset publishedAtUtc)
    {
        if (Status == PaymentReportStatus.Voided) return Result.Failure(PaymentErrors.VoidedStatusIsFinal);
        if (Status == PaymentReportStatus.Published) return Result.Failure(PaymentErrors.AlreadyPublished);
        if (_files.Count == 0) return Result.Failure(PaymentErrors.FileRequiredToPublish);

        Status = PaymentReportStatus.Published;
        PublishedAtUtc = publishedAtUtc;
        UpdatedAtUtc = publishedAtUtc;
        RaiseDomainEvent(new PaymentReportPublishedDomainEvent(Id, CompanyId, EmployeeId, TotalAmount, Currency, publishedAtUtc));
        return Result.Success();
    }

    public Result Void(string reason, DateTimeOffset voidedAtUtc)
    {
        if (Status == PaymentReportStatus.Voided) return Result.Failure(PaymentErrors.VoidedStatusIsFinal);
        if (string.IsNullOrWhiteSpace(reason)) return Result.Failure(PaymentErrors.VoidReasonRequired);
        if (reason.Trim().Length > PaymentErrors.ReasonMaxLength) return Result.Failure(PaymentErrors.ReasonTooLong);

        Status = PaymentReportStatus.Voided;
        VoidedReason = Normalize(reason);
        UpdatedAtUtc = voidedAtUtc;
        return Result.Success();
    }

    private Result EnsureDraft() => Status == PaymentReportStatus.Draft
        ? Result.Success()
        : Status == PaymentReportStatus.Voided
            ? Result.Failure(PaymentErrors.VoidedStatusIsFinal)
            : Result.Failure(PaymentErrors.DraftRequired);

    private static Result ValidateItem(Guid itemId, Guid? tripId, PaymentItemEffect effect, string description, decimal amount)
    {
        if (itemId == Guid.Empty) return Result.Failure(PaymentErrors.InvalidId);
        if (tripId == Guid.Empty) return Result.Failure(PaymentErrors.InvalidTripId);
        if (!Enum.IsDefined(effect)) return Result.Failure(PaymentErrors.InvalidEffect);
        if (string.IsNullOrWhiteSpace(description)) return Result.Failure(PaymentErrors.DescriptionRequired);
        if (description.Trim().Length > PaymentErrors.DescriptionMaxLength) return Result.Failure(PaymentErrors.DescriptionTooLong);
        if (amount < 0) return Result.Failure(PaymentErrors.InvalidAmount);
        return Result.Success();
    }

    private static Result ValidateAmountAndCurrency(decimal amount, string currency)
    {
        if (amount < 0) return Result.Failure(PaymentErrors.InvalidAmount);
        if (string.IsNullOrWhiteSpace(currency)) return Result.Failure(PaymentErrors.CurrencyRequired);
        if (currency.Trim().Length != PaymentErrors.CurrencyLength || !currency.Trim().All(char.IsLetter)) return Result.Failure(PaymentErrors.CurrencyInvalid);
        return Result.Success();
    }

    private static string Normalize(string value) => value.Trim();
    private static string NormalizeCurrency(string value) => value.Trim().ToUpperInvariant();
}
