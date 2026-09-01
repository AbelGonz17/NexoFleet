using NexoFleet.Domain.Companies;

namespace NexoFleet.Domain.UnitTests.Companies;

public sealed class CompanyNameTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateShouldFailWhenNameIsNullOrWhiteSpace(string? name)
    {
        var result = CompanyName.Create(name);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.NameRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenNameExceedsMaxLength()
    {
        var tooLongName = new string('A', CompanyName.MaxLength + 1);

        var result = CompanyName.Create(tooLongName);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldTrimAndSucceedWhenValid()
    {
        var result = CompanyName.Create("  Nexo Fleet SRL  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Nexo Fleet SRL", result.Value.Value);
        Assert.Equal("Nexo Fleet SRL", (string)result.Value);
    }
}
