using MediatR;

namespace Login.Api.Features.Schedules.Blackout;

public class CreateBlackoutDateCommand : IRequest<CreateBlackoutDateResponse>
{
    public Guid WorkshopId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string Reason { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class CreateBlackoutDateResponse
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public required string Reason { get; set; }
}

public class GetBlackoutDatesQuery : IRequest<GetBlackoutDatesResponse>
{
    public Guid WorkshopId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetBlackoutDatesResponse
{
    public required List<BlackoutDateDto> BlackoutDates { get; set; }
}

public class DeleteBlackoutDateCommand : IRequest<DeleteBlackoutDateResponse>
{
    public Guid BlackoutDateId { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class DeleteBlackoutDateResponse
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
}

public class BlackoutDateDto
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public required string Reason { get; set; }
}
