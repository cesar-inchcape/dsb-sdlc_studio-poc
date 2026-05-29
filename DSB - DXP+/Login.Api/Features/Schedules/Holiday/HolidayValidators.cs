using FluentValidation;

namespace Login.Api.Features.Schedules.Holiday;

public class CreateHolidayValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayValidator()
    {
        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("Workshop ID is required");

        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Holiday date must be today or later");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");

        RuleFor(x => x.RequestingUserRoles)
            .Must(roles => roles.Contains("SuperAdmin"))
            .WithMessage("Only SuperAdmin can create holidays");
    }
}
