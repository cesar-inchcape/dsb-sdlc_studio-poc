using MediatR;
using Login.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Features.Workshops.Update;

public class UpdateWorkshopCommandHandler : IRequestHandler<UpdateWorkshopCommand, UpdateWorkshopResponse>
{
    private readonly LoginDbContext _context;

    public UpdateWorkshopCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateWorkshopResponse> Handle(UpdateWorkshopCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can update workshops");
        }

        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        if (!workshop.IsActive)
        {
            throw new InvalidOperationException($"Cannot update inactive workshop {request.WorkshopId}");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            workshop.Name = request.Name;
        }

        if (request.Capacity.HasValue)
        {
            workshop.Capacity = request.Capacity.Value;
        }

        if (request.Address != null)
        {
            if (!string.IsNullOrWhiteSpace(request.Address.Street))
                workshop.Address!.Street = request.Address.Street;
            if (!string.IsNullOrWhiteSpace(request.Address.City))
                workshop.Address!.City = request.Address.City;
            if (!string.IsNullOrWhiteSpace(request.Address.Region))
                workshop.Address!.Region = request.Address.Region;
            if (!string.IsNullOrWhiteSpace(request.Address.PostalCode))
                workshop.Address!.PostalCode = request.Address.PostalCode;
            if (!string.IsNullOrWhiteSpace(request.Address.Country))
                workshop.Address!.Country = request.Address.Country;
        }

        workshop.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateWorkshopResponse
        {
            Id = workshop.Id,
            Name = workshop.Name,
            Brand = workshop.Brand.ToString(),
            Location = workshop.Location,
            Capacity = workshop.Capacity,
            UpdatedAt = workshop.UpdatedAt ?? DateTime.UtcNow
        };
    }
}
