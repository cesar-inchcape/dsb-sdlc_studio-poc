using FluentValidation;

namespace Login.Api.Features.Workshops.Update;

public class UpdateWorkshopValidator : AbstractValidator<UpdateWorkshopCommand>
{
    public UpdateWorkshopValidator()
    {
        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("WorkshopId is required");

        RuleFor(x => x.RequestingUserRoles)
            .NotNull().WithMessage("RequestingUserRoles is required")
            .Must(roles => roles.Contains("SuperAdmin"))
            .WithMessage("Only SuperAdmin can update workshops");

        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(255).WithMessage("Workshop name cannot exceed 255 characters");
        });

        When(x => x.Capacity.HasValue, () =>
        {
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0")
                .LessThanOrEqualTo(200).WithMessage("Capacity cannot exceed 200");
        });
    }
}
