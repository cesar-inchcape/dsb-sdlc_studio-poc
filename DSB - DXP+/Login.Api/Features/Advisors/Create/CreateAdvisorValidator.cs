using FluentValidation;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Advisors.Create;

public class CreateAdvisorValidator : AbstractValidator<CreateAdvisorCommand>
{
    public CreateAdvisorValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("Workshop ID is required");

        RuleFor(x => x.AvailableHoursPerDay)
            .GreaterThan(0).WithMessage("Available hours per day must be greater than 0")
            .LessThanOrEqualTo(24).WithMessage("Available hours per day cannot exceed 24");

        RuleFor(x => x.RequestingUserRoles)
            .Must(roles => roles.Contains("SuperAdmin"))
            .WithMessage("Only SuperAdmin can create advisors");
    }
}
