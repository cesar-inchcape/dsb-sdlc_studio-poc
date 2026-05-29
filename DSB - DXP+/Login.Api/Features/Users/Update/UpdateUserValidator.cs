using FluentValidation;

namespace Login.Api.Features.Users.Update;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.RequesterId)
            .NotEmpty().WithMessage("RequesterId is required");

        RuleFor(x => x.RequesterRoles)
            .NotNull().WithMessage("RequesterRoles is required")
            .NotEmpty().WithMessage("RequesterRoles cannot be empty");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email format is invalid");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage("FirstName must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage("LastName must not exceed 100 characters");

        RuleFor(x => x.Password)
            .MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must contain at least one digit")
            .Matches(@"[!@#$%^&*()_+=\-\[\]{};:'""\\|,.<>\/?]").When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must contain at least one special character");
    }
}
