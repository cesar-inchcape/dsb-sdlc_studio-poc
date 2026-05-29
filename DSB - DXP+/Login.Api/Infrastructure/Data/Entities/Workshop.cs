namespace Login.Api.Infrastructure.Data.Entities;

public enum WorkshopBrand
{
    Suzuki,
    Changan,
    Mazda,
    Renault,
    GWM,
    Avatr,
    Deepal,
    DSFK
}

public class Workshop
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public WorkshopBrand Brand { get; set; }
    public required string Location { get; set; }
    public Address? Address { get; set; }
    public int Capacity { get; set; } // Advisory spots per day
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<WorkshopSchedule> Schedules { get; set; } = new List<WorkshopSchedule>();
    public ICollection<WorkshopHoliday> Holidays { get; set; } = new List<WorkshopHoliday>();
    public ICollection<WorkshopBlackoutDate> BlackoutDates { get; set; } = new List<WorkshopBlackoutDate>();
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Chile";
}

public class WorkshopSchedule
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsOpen { get; set; } = true;

    // Navigation
    public Workshop Workshop { get; set; } = null!;
}

public class WorkshopHoliday
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public DateTime Date { get; set; }
    public required string Reason { get; set; }

    // Navigation
    public Workshop Workshop { get; set; } = null!;
}

public class WorkshopBlackoutDate
{
    public Guid Id { get; set; }
    public Guid WorkshopId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string Reason { get; set; }

    // Navigation
    public Workshop Workshop { get; set; } = null!;
}
