using FluentValidation.Results;
using NexoFleet.Domain.Common;

namespace NexoFleet.Application.Common;

public static class ValidationExtensions
{
    public static ValidationError ToValidationError(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

        return new ValidationError(errors);
    }
}
