namespace Login.Api.Infrastructure.Data.Entities;

/// <summary>
/// Advisor brand assignments for multi-brand support
/// </summary>
public enum AdvisorBrand
{
    Suzuki = 0,
    Changan = 1,
    Mazda = 2,
    Renault = 3,
    GWM = 4,
    Avatr = 5,
    Deepal = 6,
    DSFK = 7
}

/// <summary>
/// Advisor entity - sales consultants assigned to workshops and brands
/// </summary>
public class Advisor
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    
    /// <summary>
    /// Assigned workshop ID (advisor works at specific workshop)
    /// </summary>
    public Guid WorkshopId { get; set; }
    
    /// <summary>
    /// Assigned brand (advisor specializes in one brand)
    /// </summary>
    public AdvisorBrand AssignedBrand { get; set; }
    
    /// <summary>
    /// Availability status (active/inactive)
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Available hours per day (for scheduling)
    /// </summary>
    public int AvailableHoursPerDay { get; set; } = 8;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Workshop? Workshop { get; set; }
}
