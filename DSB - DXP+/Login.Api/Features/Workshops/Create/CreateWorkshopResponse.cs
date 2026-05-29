namespace Login.Api.Features.Workshops.Create;

public class CreateWorkshopResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Brand { get; set; }
    public required string Location { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
}
