using MediatR;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Schedules.Blackout;

public class CreateBlackoutDateCommandHandler : IRequestHandler<CreateBlackoutDateCommand, CreateBlackoutDateResponse>
{
    private readonly LoginDbContext _context;

    public CreateBlackoutDateCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<CreateBlackoutDateResponse> Handle(CreateBlackoutDateCommand request, CancellationToken cancellationToken)
    {
        // Authorization
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can create blackout dates");
        }

        // Validate dates
        if (request.StartDate.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Start date must be today or later");
        }

        if (request.EndDate.Date < request.StartDate.Date)
        {
            throw new InvalidOperationException("End date must be same as or after start date");
        }

        // Verify workshop exists
        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        // Check for overlapping blackout dates
        var overlapping = _context.WorkshopBlackoutDates
            .Where(b => b.WorkshopId == request.WorkshopId &&
                   b.StartDate <= request.EndDate &&
                   b.EndDate >= request.StartDate)
            .FirstOrDefault();
        
        if (overlapping != null)
        {
            throw new InvalidOperationException($"Overlapping blackout date exists for {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}");
        }

        var blackoutDate = new WorkshopBlackoutDate
        {
            Id = Guid.NewGuid(),
            WorkshopId = request.WorkshopId,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            Reason = request.Reason
        };

        _context.WorkshopBlackoutDates.Add(blackoutDate);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateBlackoutDateResponse
        {
            Id = blackoutDate.Id,
            WorkshopId = blackoutDate.WorkshopId,
            StartDate = blackoutDate.StartDate.ToString("yyyy-MM-dd"),
            EndDate = blackoutDate.EndDate.ToString("yyyy-MM-dd"),
            Reason = blackoutDate.Reason
        };
    }
}

public class GetBlackoutDatesQueryHandler : IRequestHandler<GetBlackoutDatesQuery, GetBlackoutDatesResponse>
{
    private readonly LoginDbContext _context;

    public GetBlackoutDatesQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<GetBlackoutDatesResponse> Handle(GetBlackoutDatesQuery request, CancellationToken cancellationToken)
    {
        // Verify workshop exists
        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        var query = _context.WorkshopBlackoutDates.Where(b => b.WorkshopId == request.WorkshopId);

        if (request.FromDate.HasValue)
        {
            query = query.Where(b => b.EndDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(b => b.StartDate <= request.ToDate.Value.Date);
        }

        var blackoutDates = await Task.Run(() => query.OrderBy(b => b.StartDate).ToList(), cancellationToken);

        return new GetBlackoutDatesResponse
        {
            BlackoutDates = blackoutDates.Select(b => new BlackoutDateDto
            {
                Id = b.Id,
                WorkshopId = b.WorkshopId,
                StartDate = b.StartDate.ToString("yyyy-MM-dd"),
                EndDate = b.EndDate.ToString("yyyy-MM-dd"),
                Reason = b.Reason
            }).ToList()
        };
    }
}

public class DeleteBlackoutDateCommandHandler : IRequestHandler<DeleteBlackoutDateCommand, DeleteBlackoutDateResponse>
{
    private readonly LoginDbContext _context;

    public DeleteBlackoutDateCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteBlackoutDateResponse> Handle(DeleteBlackoutDateCommand request, CancellationToken cancellationToken)
    {
        // Authorization
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can delete blackout dates");
        }

        var blackoutDate = await _context.WorkshopBlackoutDates.FindAsync(new object?[] { request.BlackoutDateId }, cancellationToken: cancellationToken);
        if (blackoutDate == null)
        {
            throw new InvalidOperationException($"Blackout date {request.BlackoutDateId} not found");
        }

        _context.WorkshopBlackoutDates.Remove(blackoutDate);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteBlackoutDateResponse
        {
            Id = blackoutDate.Id,
            Message = $"Blackout date from {blackoutDate.StartDate:yyyy-MM-dd} to {blackoutDate.EndDate:yyyy-MM-dd} has been deleted"
        };
    }
}
