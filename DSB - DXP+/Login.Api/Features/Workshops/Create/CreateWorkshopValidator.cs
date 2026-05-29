using FluentValidation;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Workshops.Create;

public class CreateWorkshopValidator : AbstractValidator<CreateWorkshopCommand>
{
    public CreateWorkshopValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workshop name is required")
            .MaximumLength(255).WithMessage("Workshop name cannot exceed 255 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Workshop location is required")
            .MaximumLength(255).WithMessage("Location cannot exceed 255 characters");

        RuleFor(x => x.Brand)
            .IsInEnum().WithMessage("Invalid workshop brand");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0")
            .LessThanOrEqualTo(200).WithMessage("Capacity cannot exceed 200");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address is required")
            .Must(ValidateAddress).WithMessage("Address is incomplete");

        RuleFor(x => x.RequestingUserRoles)
            .NotNull().WithMessage("RequestingUserRoles is required")
            .Must(roles => roles.Contains("SuperAdmin"))
            .WithMessage("Only SuperAdmin can create workshops");
    }

    private bool ValidateAddress(CreateWorkshopAddress? address)
    {
        if (address == null) return false;
        return !string.IsNullOrWhiteSpace(address.Street) &&
               !string.IsNullOrWhiteSpace(address.City) &&
               !string.IsNullOrWhiteSpace(address.Region) &&
               !string.IsNullOrWhiteSpace(address.PostalCode);
    }
}
