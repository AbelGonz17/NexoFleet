using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.UnitTests.Common;

public sealed class EmailTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateShouldFailWhenEmailIsNullOrWhiteSpace(string? email)
    {
        var result = Email.Create(email);

        Assert.True(result.IsFailure);
        Assert.Equal(SharedErrors.EmailInvalid, result.Error);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid@.com")]
    public void CreateShouldFailWhenEmailFormatIsInvalid(string email)
    {
        var result = Email.Create(email);

        Assert.True(result.IsFailure);
        Assert.Equal(SharedErrors.EmailInvalid, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenEmailExceedsMaxLength()
    {
        var prefix = new string('a', Email.MaxLength);
        var tooLongEmail = $"{prefix}@example.com";

        var result = Email.Create(tooLongEmail);

        Assert.True(result.IsFailure);
        Assert.Equal(SharedErrors.EmailTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldNormalizeToLowerAndTrimWhenValid()
    {
        var result = Email.Create("  Admin.Transport@NexoFleet.COM  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("admin.transport@nexofleet.com", result.Value.Value);
        Assert.Equal("admin.transport@nexofleet.com", (string)result.Value);
    }

    [Fact]
    public void CreateShouldUseCustomErrorWhenProvided()
    {
        var customError = Error.Validation("Custom.Email", "Custom error");

        var result = Email.Create("invalid", invalidError: customError);

        Assert.True(result.IsFailure);
        Assert.Equal(customError, result.Error);
    }
}
