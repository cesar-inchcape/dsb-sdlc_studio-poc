using MediatR;

namespace Login.Api.Features.Schedules.WorkshopSchedule;

public class UpdateWorkshopScheduleCommand : IRequest<UpdateWorkshopScheduleResponse>
{
    public Guid WorkshopId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsOpen { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class UpdateWorkshopScheduleResponse
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
}

public class GetWorkshopScheduleQuery : IRequest<GetWorkshopScheduleResponse>
{
    public Guid WorkshopId { get; set; }
}

public class GetWorkshopScheduleResponse
{
    public required List<WorkshopScheduleDto> Schedules { get; set; }
}

public class WorkshopScheduleDto
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
}
