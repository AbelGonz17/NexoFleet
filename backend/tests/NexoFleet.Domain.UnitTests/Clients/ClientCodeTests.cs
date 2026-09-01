using NexoFleet.Domain.Clients;

namespace NexoFleet.Domain.UnitTests.Clients;

public sealed class ClientCodeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateShouldFailWhenCodeIsNullOrWhiteSpace(string? code)
    {
        var result = ClientCode.Create(code);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.ClientCodeRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenCodeExceedsMaxLength()
    {
        var tooLongCode = new string('A', ClientCode.MaxLength + 1);

        var result = ClientCode.Create(tooLongCode);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.ClientCodeTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldNormalizeToUpperAndTrimWhenValid()
    {
        var result = ClientCode.Create("  cli-001  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("CLI-001", result.Value.Value);
        Assert.Equal("CLI-001", (string)result.Value);
    }
}
