using MediatR;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;

namespace Login.Api.Features.Schedules.Holiday;

public class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, CreateHolidayResponse>
{
    private readonly LoginDbContext _context;

    public CreateHolidayCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<CreateHolidayResponse> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        // Authorization
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can create holidays");
        }

        // Validate date
        if (request.Date.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Holiday date must be today or later");
        }

        // Verify workshop exists
        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        // Check duplicate holiday
        var existingHoliday = _context.WorkshopHolidays
            .FirstOrDefault(h => h.WorkshopId == request.WorkshopId && h.Date.Date == request.Date.Date);
        if (existingHoliday != null)
        {
            throw new InvalidOperationException($"Holiday already exists for {request.Date:yyyy-MM-dd}");
        }

        var holiday = new WorkshopHoliday
        {
            Id = Guid.NewGuid(),
            WorkshopId = request.WorkshopId,
            Date = request.Date.Date,
            Reason = request.Reason
        };

        _context.WorkshopHolidays.Add(holiday);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateHolidayResponse
        {
            Id = holiday.Id,
            WorkshopId = holiday.WorkshopId,
            Date = holiday.Date.ToString("yyyy-MM-dd"),
            Reason = holiday.Reason
        };
    }
}

public class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, GetHolidaysResponse>
{
    private readonly LoginDbContext _context;

    public GetHolidaysQueryHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<GetHolidaysResponse> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
    {
        // Verify workshop exists
        var workshop = await _context.Workshops.FindAsync(new object?[] { request.WorkshopId }, cancellationToken: cancellationToken);
        if (workshop == null)
        {
            throw new InvalidOperationException($"Workshop {request.WorkshopId} not found");
        }

        var query = _context.WorkshopHolidays.Where(h => h.WorkshopId == request.WorkshopId);

        if (request.FromDate.HasValue)
        {
            query = query.Where(h => h.Date >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(h => h.Date <= request.ToDate.Value.Date);
        }

        var holidays = await Task.Run(() => query.OrderBy(h => h.Date).ToList(), cancellationToken);

        return new GetHolidaysResponse
        {
            Holidays = holidays.Select(h => new HolidayDto
            {
                Id = h.Id,
                WorkshopId = h.WorkshopId,
                Date = h.Date.ToString("yyyy-MM-dd"),
                Reason = h.Reason
            }).ToList()
        };
    }
}

public class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, DeleteHolidayResponse>
{
    private readonly LoginDbContext _context;

    public DeleteHolidayCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteHolidayResponse> Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        // Authorization
        if (!request.RequestingUserRoles.Contains("SuperAdmin"))
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can delete holidays");
        }

        var holiday = await _context.WorkshopHolidays.FindAsync(new object?[] { request.HolidayId }, cancellationToken: cancellationToken);
        if (holiday == null)
        {
            throw new InvalidOperationException($"Holiday {request.HolidayId} not found");
        }

        _context.WorkshopHolidays.Remove(holiday);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteHolidayResponse
        {
            Id = holiday.Id,
            Message = $"Holiday on {holiday.Date:yyyy-MM-dd} has been deleted"
        };
    }
}
