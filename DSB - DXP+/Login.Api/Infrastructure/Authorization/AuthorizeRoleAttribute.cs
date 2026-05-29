using Microsoft.AspNetCore.Authorization;

namespace Login.Api.Infrastructure.Authorization;

/// <summary>
/// Custom attribute for role-based authorization.
/// Usage: [AuthorizeRole(nameof(AuthorizationPolicies.SuperAdminOnly))]
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Creates an authorization requirement for the specified policy.
    /// </summary>
    /// <param name="policy">Policy name from AuthorizationPolicies</param>
    public AuthorizeRoleAttribute(string policy)
    {
        Policy = policy;
    }

    /// <summary>
    /// Creates an authorization requirement for SuperAdmin only.
    /// </summary>
    public static class Policies
    {
        public static AuthorizeRoleAttribute SuperAdminOnly =>
            new(AuthorizationPolicies.SuperAdminOnly);

        public static AuthorizeRoleAttribute AdminOrHigher =>
            new(AuthorizationPolicies.AdminOrHigher);

        public static AuthorizeRoleAttribute WorkshopReadOnly =>
            new(AuthorizationPolicies.WorkshopReadOnly);
    }
}
