using MediatR;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Workshops.Create;

public class CreateWorkshopCommandHandler : IRequestHandler<CreateWorkshopCommand, CreateWorkshopResponse>
{
    private readonly LoginDbContext _context;

    public CreateWorkshopCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<CreateWorkshopResponse> Handle(CreateWorkshopCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can create workshops");
        }

        // Check if workshop with same name and brand already exists
        var existingWorkshop = _context.Workshops
            .FirstOrDefault(w => w.Name == request.Name && w.Brand == request.Brand);

        if (existingWorkshop != null)
        {
            throw new InvalidOperationException($"Workshop '{request.Name}' for brand {request.Brand} already exists");
        }

        // Check if location is unique per brand
        var locationExists = _context.Workshops
            .Any(w => w.Location == request.Location && w.Brand == request.Brand);

        if (locationExists)
        {
            throw new InvalidOperationException($"A workshop already exists at location '{request.Location}' for brand {request.Brand}");
        }

        // Create workshop
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Brand = request.Brand,
            Location = request.Location,
            Capacity = request.Capacity,
            Address = new Infrastructure.Data.Entities.Address
            {
                Street = request.Address.Street,
                City = request.Address.City,
                Region = request.Address.Region,
                PostalCode = request.Address.PostalCode,
                Country = request.Address.Country
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Initialize default operating hours (08:00-18:00, all days open)
        for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            _context.WorkshopSchedules.Add(new WorkshopSchedule
            {
                Id = Guid.NewGuid(),
                WorkshopId = workshop.Id,
                DayOfWeek = (DayOfWeek)dayOfWeek,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(18, 0, 0),
                IsOpen = true
            });
        }

        _context.Workshops.Add(workshop);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateWorkshopResponse
        {
            Id = workshop.Id,
            Name = workshop.Name,
            Brand = workshop.Brand.ToString(),
            Location = workshop.Location,
            Capacity = workshop.Capacity,
            CreatedAt = workshop.CreatedAt
        };
    }
}
