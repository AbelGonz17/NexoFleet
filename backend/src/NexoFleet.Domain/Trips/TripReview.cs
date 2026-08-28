using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips;

public sealed class TripReview : Entity
{
    internal TripReview(
        Guid id,
        Guid tripId,
        Guid companyId,
        Guid reviewerUserId,
        TripReviewDecision decision,
        string? comments,
        DateTimeOffset reviewedAtUtc) : base(id)
    {
        TripId = tripId;
        CompanyId = companyId;
        ReviewerUserId = reviewerUserId;
        Decision = decision;
        Comments = comments;
        ReviewedAtUtc = reviewedAtUtc;
    }

    private TripReview() { }

    public Guid TripId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid ReviewerUserId { get; private set; }
    public TripReviewDecision Decision { get; private set; }
    public string? Comments { get; private set; }
    public DateTimeOffset ReviewedAtUtc { get; private set; }
}
