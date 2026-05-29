using MediatR;
using Login.Api.Infrastructure.Data;

namespace Login.Api.Features.Advisors.Delete;

public class DeleteAdvisorCommandHandler : IRequestHandler<DeleteAdvisorCommand, DeleteAdvisorResponse>
{
    private readonly LoginDbContext _context;

    public DeleteAdvisorCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteAdvisorResponse> Handle(DeleteAdvisorCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can delete advisors");
        }

        var advisor = await _context.Advisors.FindAsync(new object?[] { request.AdvisorId }, cancellationToken: cancellationToken);
        if (advisor == null)
        {
            throw new InvalidOperationException($"Advisor {request.AdvisorId} not found");
        }

        if (!advisor.IsActive)
        {
            throw new InvalidOperationException($"Advisor is already inactive");
        }

        // Soft delete
        advisor.IsActive = false;
        advisor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteAdvisorResponse
        {
            Id = advisor.Id,
            Message = $"Advisor {advisor.FirstName} {advisor.LastName} has been deactivated"
        };
    }
}
