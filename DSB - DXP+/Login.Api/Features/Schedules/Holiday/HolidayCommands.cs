using MediatR;

namespace Login.Api.Features.Schedules.Holiday;

public class CreateHolidayCommand : IRequest<CreateHolidayResponse>
{
    public Guid WorkshopId { get; set; }
    public DateTime Date { get; set; }
    public required string Reason { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class CreateHolidayResponse
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public string Date { get; set; } = string.Empty;
    public required string Reason { get; set; }
}

public class GetHolidaysQuery : IRequest<GetHolidaysResponse>
{
    public Guid WorkshopId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetHolidaysResponse
{
    public required List<HolidayDto> Holidays { get; set; }
}

public class DeleteHolidayCommand : IRequest<DeleteHolidayResponse>
{
    public Guid HolidayId { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> RequestingUserRoles { get; set; } = new();
}

public class DeleteHolidayResponse
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
}

public class HolidayDto
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public string Date { get; set; } = string.Empty;
    public required string Reason { get; set; }
}
