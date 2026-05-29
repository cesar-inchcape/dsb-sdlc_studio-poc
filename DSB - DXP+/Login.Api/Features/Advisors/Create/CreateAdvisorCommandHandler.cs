using MediatR;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Advisors.Create;

public class CreateAdvisorCommandHandler : IRequestHandler<CreateAdvisorCommand, CreateAdvisorResponse>
{
    private readonly LoginDbContext _context;

    public CreateAdvisorCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<CreateAdvisorResponse> Handle(CreateAdvisorCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can create advisors");
        }

        // Verify workshop exists
        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        // Check email uniqueness
        var existingAdvisor = _context.Advisors.FirstOrDefault(a => a.Email == request.Email);
        if (existingAdvisor != null)
        {
            throw new InvalidOperationException($"Advisor with email {request.Email} already exists");
        }

        var advisor = new Advisor
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            WorkshopId = request.WorkshopId,
            AssignedBrand = request.AssignedBrand,
            AvailableHoursPerDay = request.AvailableHoursPerDay,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Advisors.Add(advisor);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateAdvisorResponse
        {
            Id = advisor.Id,
            FirstName = advisor.FirstName,
            LastName = advisor.LastName,
            Email = advisor.Email,
            PhoneNumber = advisor.PhoneNumber,
            WorkshopId = advisor.WorkshopId,
            AssignedBrand = advisor.AssignedBrand.ToString(),
            AvailableHoursPerDay = advisor.AvailableHoursPerDay,
            CreatedAt = advisor.CreatedAt
        };
    }
}
