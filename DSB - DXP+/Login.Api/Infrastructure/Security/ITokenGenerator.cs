using System.Security.Claims;

namespace Login.Api.Infrastructure.Security;

public interface ITokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, string[] roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
