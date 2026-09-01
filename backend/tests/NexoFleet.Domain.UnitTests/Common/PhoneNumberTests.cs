using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.UnitTests.Common;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateShouldFailWhenPhoneIsNullOrWhiteSpace(string? phone)
    {
        var result = PhoneNumber.Create(phone);

        Assert.True(result.IsFailure);
        Assert.Equal(SharedErrors.PhoneRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenPhoneExceedsMaxLength()
    {
        var tooLongPhone = new string('1', PhoneNumber.MaxLength + 1);

        var result = PhoneNumber.Create(tooLongPhone);

        Assert.True(result.IsFailure);
        Assert.Equal(SharedErrors.PhoneTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldTrimAndSucceedWhenValid()
    {
        var result = PhoneNumber.Create("  +591 700 00000  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("+591 700 00000", result.Value.Value);
        Assert.Equal("+591 700 00000", (string)result.Value);
    }

    [Fact]
    public void CreateShouldUseCustomErrorWhenProvided()
    {
        var customError = Error.Validation("Custom.Phone", "Custom error");

        var result = PhoneNumber.Create("", requiredError: customError);

        Assert.True(result.IsFailure);
        Assert.Equal(customError, result.Error);
    }
}
