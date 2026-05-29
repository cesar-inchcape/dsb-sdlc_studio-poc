using FluentValidation;

namespace Login.Api.Features.Users.AssignRole;

public class AssignRoleValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("RequestingUserId is required");

        RuleFor(x => x.RequestingUserRoles)
            .NotNull().WithMessage("RequestingUserRoles is required")
            .Must(roles => roles.Contains("SuperAdmin"))
            .WithMessage("Only SuperAdmin can assign roles");
    }
}
