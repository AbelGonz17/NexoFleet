using NexoFleet.Domain.Employees;

namespace NexoFleet.Domain.UnitTests.Employees;

public sealed class IdentityDocumentTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateShouldFailWhenDocumentIsNullOrWhiteSpace(string? document)
    {
        var result = IdentityDocument.Create(document);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.IdentityDocumentRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenDocumentExceedsMaxLength()
    {
        var tooLongDoc = new string('A', IdentityDocument.MaxLength + 1);

        var result = IdentityDocument.Create(tooLongDoc);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.IdentityDocumentTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldNormalizeToUpperAndTrimWhenValid()
    {
        var result = IdentityDocument.Create("  ci-123456-lp  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("CI-123456-LP", result.Value.Value);
        Assert.Equal("CI-123456-LP", (string)result.Value);
    }
}
