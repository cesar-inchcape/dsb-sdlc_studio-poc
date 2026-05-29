using FluentValidation;

namespace Login.Api.Features.Schedules.WorkshopSchedule;

public class UpdateWorkshopScheduleValidator : AbstractValidator<UpdateWorkshopScheduleCommand>
{
    public UpdateWorkshopScheduleValidator()
    {
        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("Workshop ID is required");

        RuleFor(x => x.StartTime)
            .Must(time => time < TimeSpan.FromHours(24))
            .WithMessage("Start time must be valid (0-23:59:59)");

        RuleFor(x => x.EndTime)
            .Must(time => time <= TimeSpan.FromHours(24))
            .WithMessage("End time must be valid (0-23:59:59)")
            .GreaterThan(x => x.StartTime)
            .When(x => x.IsOpen)
            .WithMessage("End time must be after start time when workshop is open");

        RuleFor(x => x.RequestingUserRoles)
            .Must(roles => roles.Contains("SuperAdmin"))
            .WithMessage("Only SuperAdmin can update workshop schedules");
    }
}
