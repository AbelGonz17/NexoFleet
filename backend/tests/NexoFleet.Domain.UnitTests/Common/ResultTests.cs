using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void SuccessShouldExposeItsValue()
    {
        var result = Result<string>.Success("NexoFleet");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("NexoFleet", result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void FailureShouldExposeItsError()
    {
        var error = Error.NotFound("Test.NotFound", "No encontrado.");

        var result = Result<string>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ReadingValueFromFailureShouldThrow()
    {
        var result = Result<string>.Failure(
            Error.Failure("Test.Failure", "Falló."));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
