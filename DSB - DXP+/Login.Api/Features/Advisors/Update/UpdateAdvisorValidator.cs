using FluentValidation;

namespace Login.Api.Features.Advisors.Update;

public class UpdateAdvisorValidator : AbstractValidator<UpdateAdvisorCommand>
{
    public UpdateAdvisorValidator()
    {
        RuleFor(x => x.AdvisorId)
            .NotEmpty().WithMessage("Advisor ID is required");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email must be valid")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.AvailableHoursPerDay)
            .GreaterThan(0).WithMessage("Available hours per day must be greater than 0")
            .LessThanOrEqualTo(24).WithMessage("Available hours per day cannot exceed 24")
            .When(x => x.AvailableHoursPerDay.HasValue);

        RuleFor(x => x.RequestingUserRoles)
            .Must(roles => roles.Contains("SuperAdmin"))
            .WithMessage("Only SuperAdmin can update advisors");
    }
}
