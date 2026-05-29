using MediatR;
using Login.Api.Infrastructure.Data;

namespace Login.Api.Features.Advisors.Update;

public class UpdateAdvisorCommandHandler : IRequestHandler<UpdateAdvisorCommand, UpdateAdvisorResponse>
{
    private readonly LoginDbContext _context;

    public UpdateAdvisorCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateAdvisorResponse> Handle(UpdateAdvisorCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can update advisors");
        }

        var advisor = await _context.Advisors.FindAsync(new object?[] { request.AdvisorId }, cancellationToken: cancellationToken);
        if (advisor == null)
        {
            throw new InvalidOperationException($"Advisor {request.AdvisorId} not found");
        }

        if (!advisor.IsActive)
        {
            throw new InvalidOperationException($"Cannot update inactive advisor");
        }

        // Check email uniqueness if changed
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != advisor.Email)
        {
            var existingAdvisor = _context.Advisors.FirstOrDefault(a => a.Email == request.Email);
            if (existingAdvisor != null)
            {
                throw new InvalidOperationException($"Email {request.Email} is already in use");
            }
        }

        // Update provided fields
        if (!string.IsNullOrWhiteSpace(request.FirstName))
            advisor.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            advisor.LastName = request.LastName;

        if (!string.IsNullOrWhiteSpace(request.Email))
            advisor.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            advisor.PhoneNumber = request.PhoneNumber;

        if (request.AssignedBrand.HasValue)
            advisor.AssignedBrand = request.AssignedBrand.Value;

        if (request.AvailableHoursPerDay.HasValue)
            advisor.AvailableHoursPerDay = request.AvailableHoursPerDay.Value;

        advisor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateAdvisorResponse
        {
            Id = advisor.Id,
            FirstName = advisor.FirstName,
            LastName = advisor.LastName,
            Email = advisor.Email,
            PhoneNumber = advisor.PhoneNumber,
            WorkshopId = advisor.WorkshopId,
            AssignedBrand = advisor.AssignedBrand.ToString(),
            AvailableHoursPerDay = advisor.AvailableHoursPerDay,
            UpdatedAt = advisor.UpdatedAt
        };
    }
}
