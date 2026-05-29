using FluentValidation;

namespace Login.Api.Features.Users.Create;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required")
            .MaximumLength(100).WithMessage("FirstName must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .MaximumLength(100).WithMessage("LastName must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
            .Matches(@"[!@#$%^&*()_+=\-\[\]{};:'""\\|,.<>\/?]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.RoleIds)
            .Must(roleIds => roleIds == null || roleIds.Count == 0 || roleIds.All(id => id != Guid.Empty))
            .WithMessage("Invalid role IDs provided");
    }
}
