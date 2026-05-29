using FluentValidation;

namespace Login.Api.Features.Schedules.Blackout;

public class CreateBlackoutDateValidator : AbstractValidator<CreateBlackoutDateCommand>
{
    public CreateBlackoutDateValidator()
    {
        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("Workshop ID is required");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Start date must be today or later");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be same as or after start date");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");

        RuleFor(x => x.RequestingUserRoles)
            .Must(roles => roles.Contains("SuperAdmin"))
            .WithMessage("Only SuperAdmin can create blackout dates");
    }
}
