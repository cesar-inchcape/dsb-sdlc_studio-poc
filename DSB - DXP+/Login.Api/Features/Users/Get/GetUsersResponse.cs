namespace Login.Api.Features.Users.Get;

public class GetUsersResponse
{
    public List<UserDto> Users { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
