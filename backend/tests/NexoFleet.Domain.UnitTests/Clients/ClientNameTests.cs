using NexoFleet.Domain.Clients;

namespace NexoFleet.Domain.UnitTests.Clients;

public sealed class ClientNameTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateShouldFailWhenNameIsNullOrWhiteSpace(string? name)
    {
        var result = ClientName.Create(name);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.NameRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenNameExceedsMaxLength()
    {
        var tooLongName = new string('A', ClientName.MaxLength + 1);

        var result = ClientName.Create(tooLongName);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldTrimAndSucceedWhenValid()
    {
        var result = ClientName.Create("  Acme Logistics SRL  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Acme Logistics SRL", result.Value.Value);
        Assert.Equal("Acme Logistics SRL", (string)result.Value);
    }
}
