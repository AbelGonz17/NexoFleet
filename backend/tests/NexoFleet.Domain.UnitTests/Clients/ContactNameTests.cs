using NexoFleet.Domain.Clients;

namespace NexoFleet.Domain.UnitTests.Clients;

public sealed class ContactNameTests
{
    [Fact]
    public void CreateShouldFailWhenContactNameExceedsMaxLength()
    {
        var tooLongName = new string('A', ContactName.MaxLength + 1);

        var result = ContactName.Create(tooLongName);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.ContactNameTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldTrimAndSucceedWhenValid()
    {
        var result = ContactName.Create("  Jane Doe  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Jane Doe", result.Value.Value);
        Assert.Equal("Jane Doe", (string)result.Value);
    }
}
