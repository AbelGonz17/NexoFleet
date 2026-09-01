using NexoFleet.Domain.Employees;

namespace NexoFleet.Domain.UnitTests.Employees;

public sealed class EmployeeCodeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateShouldFailWhenCodeIsNullOrWhiteSpace(string? code)
    {
        var result = EmployeeCode.Create(code);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.EmployeeCodeRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenCodeExceedsMaxLength()
    {
        var tooLongCode = new string('A', EmployeeCode.MaxLength + 1);

        var result = EmployeeCode.Create(tooLongCode);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.EmployeeCodeTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldNormalizeToUpperAndTrimWhenValid()
    {
        var result = EmployeeCode.Create("  emp-001  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("EMP-001", result.Value.Value);
        Assert.Equal("EMP-001", (string)result.Value);
    }
}
