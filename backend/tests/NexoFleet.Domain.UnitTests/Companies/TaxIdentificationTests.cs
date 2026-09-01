using NexoFleet.Domain.Companies;

namespace NexoFleet.Domain.UnitTests.Companies;

public sealed class TaxIdentificationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateShouldFailWhenTaxIdIsNullOrWhiteSpace(string? taxId)
    {
        var result = TaxIdentification.Create(taxId);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.TaxIdentificationRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenTaxIdExceedsMaxLength()
    {
        var tooLongTaxId = new string('A', TaxIdentification.MaxLength + 1);

        var result = TaxIdentification.Create(tooLongTaxId);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.TaxIdentificationTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldNormalizeToUpperAndTrimWhenValid()
    {
        var result = TaxIdentification.Create("  bo-123456-abc  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("BO-123456-ABC", result.Value.Value);
        Assert.Equal("BO-123456-ABC", (string)result.Value);
    }
}
