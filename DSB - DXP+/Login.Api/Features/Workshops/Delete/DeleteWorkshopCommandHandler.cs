using MediatR;
using Login.Api.Infrastructure.Data;

namespace Login.Api.Features.Workshops.Delete;

public class DeleteWorkshopCommandHandler : IRequestHandler<DeleteWorkshopCommand, DeleteWorkshopResponse>
{
    private readonly LoginDbContext _context;

    public DeleteWorkshopCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteWorkshopResponse> Handle(DeleteWorkshopCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can delete workshops");
        }

        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        if (!workshop.IsActive)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} is already inactive");
        }

        // Soft delete
        workshop.IsActive = false;
        workshop.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteWorkshopResponse
        {
            Id = workshop.Id,
            Message = $"Workshop {workshop.Name} has been deactivated"
        };
    }
}
