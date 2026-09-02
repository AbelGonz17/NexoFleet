using FluentValidation;
using NexoFleet.Application.Employees.Dtos;
using NexoFleet.Domain.Employees;

namespace NexoFleet.Application.Employees.Validators;

public sealed class LinkUserAccountRequestValidator : AbstractValidator<LinkUserAccountRequest>
{
    public LinkUserAccountRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty().WithMessage(EmployeeErrors.InvalidUserId.Description);
    }
}
