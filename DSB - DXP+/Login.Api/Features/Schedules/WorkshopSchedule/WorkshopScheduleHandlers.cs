using MediatR;
using Login.Api.Infrastructure.Data;

namespace Login.Api.Features.Schedules.WorkshopSchedule;

public class UpdateWorkshopScheduleCommandHandler : IRequestHandler<UpdateWorkshopScheduleCommand, UpdateWorkshopScheduleResponse>
{
    private readonly LoginDbContext _context;

    public UpdateWorkshopScheduleCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateWorkshopScheduleResponse> Handle(UpdateWorkshopScheduleCommand request, CancellationToken cancellationToken)
    {
        // Authorization
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can update workshop schedules");
        }

        // Verify workshop exists
        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        // Find or create schedule for this day
        var schedule = _context.WorkshopSchedules
            .FirstOrDefault(s => s.WorkshopId == request.WorkshopId && s.DayOfWeek == request.DayOfWeek);

        if (schedule == null)
        {
            schedule = new Infrastructure.Data.Entities.WorkshopSchedule
            {
                Id = Guid.NewGuid(),
                WorkshopId = request.WorkshopId,
                DayOfWeek = request.DayOfWeek
            };
            _context.WorkshopSchedules.Add(schedule);
        }

        // Update schedule
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.IsOpen = request.IsOpen;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateWorkshopScheduleResponse
        {
            Id = schedule.Id,
            WorkshopId = schedule.WorkshopId,
            DayOfWeek = schedule.DayOfWeek.ToString(),
            StartTime = schedule.StartTime.ToString(@"hh\:mm"),
            EndTime = schedule.EndTime.ToString(@"hh\:mm"),
            IsOpen = schedule.IsOpen
        };
    }
}

public class GetWorkshopScheduleQueryHandler : IRequestHandler<GetWorkshopScheduleQuery, GetWorkshopScheduleResponse>
{
    private readonly LoginDbContext _context;

    public GetWorkshopScheduleQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<GetWorkshopScheduleResponse> Handle(GetWorkshopScheduleQuery request, CancellationToken cancellationToken)
    {
        // Verify workshop exists
        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        var schedules = _context.WorkshopSchedules
            .Where(s => s.WorkshopId == request.WorkshopId)
            .OrderBy(s => s.DayOfWeek)
            .ToList();

        return new GetWorkshopScheduleResponse
        {
            Schedules = schedules.Select(s => new WorkshopScheduleDto
            {
                Id = s.Id,
                WorkshopId = s.WorkshopId,
                DayOfWeek = s.DayOfWeek.ToString(),
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                IsOpen = s.IsOpen
            }).ToList()
        };
    }
}
