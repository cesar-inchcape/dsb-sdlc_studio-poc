using MediatR;

namespace Login.Api.Features.Users.Get;

public class GetUsersQuery : IRequest<GetUsersResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? EmailFilter { get; set; }
    public string? FirstNameFilter { get; set; }
    public string? LastNameFilter { get; set; }
}
