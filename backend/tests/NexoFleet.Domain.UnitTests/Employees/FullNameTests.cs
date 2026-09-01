using NexoFleet.Domain.Employees;

namespace NexoFleet.Domain.UnitTests.Employees;

public sealed class FullNameTests
{
    [Theory]
    [InlineData(null, "González")]
    [InlineData("", "González")]
    [InlineData("   ", "González")]
    public void CreateShouldFailWhenFirstNameIsNullOrWhiteSpace(string? firstName, string? lastName)
    {
        var result = FullName.Create(firstName, lastName);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.FirstNameRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenFirstNameExceedsMaxLength()
    {
        var tooLongFirstName = new string('A', FullName.FirstNameMaxLength + 1);

        var result = FullName.Create(tooLongFirstName, "González");

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.FirstNameTooLong, result.Error);
    }

    [Theory]
    [InlineData("Abel", null)]
    [InlineData("Abel", "")]
    [InlineData("Abel", "   ")]
    public void CreateShouldFailWhenLastNameIsNullOrWhiteSpace(string? firstName, string? lastName)
    {
        var result = FullName.Create(firstName, lastName);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.LastNameRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenLastNameExceedsMaxLength()
    {
        var tooLongLastName = new string('A', FullName.LastNameMaxLength + 1);

        var result = FullName.Create("Abel", tooLongLastName);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.LastNameTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldTrimAndSucceedWhenValid()
    {
        var result = FullName.Create("  Abel  ", "  González  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Abel", result.Value.FirstName);
        Assert.Equal("González", result.Value.LastName);
        Assert.Equal("Abel González", result.Value.ToString());
    }
}
