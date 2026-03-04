using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

/// <summary>
/// Service for extracting current user context from JWT claims.
/// This is testable because it only accesses HttpContext for claim extraction.
/// Security: Never accepts user ID from request body - always reads from claims.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current user's ID from JWT 'sub' claim.
    /// Returns null if user is not authenticated.
    /// </summary>

    public string UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User == null)
                return string.Empty;

            // Try "sub" claim first (standard JWT)
            var subClaim = httpContext.User.FindFirst("sub");
            if (subClaim != null)
                return subClaim.Value;

            // Fallback to NameIdentifier
            var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null)
                return idClaim.Value;

            // Last resort: try "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            var legacyClaim = httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            return legacyClaim?.Value ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the current user's role from JWT 'role' claim.
    /// Returns null if user is not authenticated or role claim is missing.
    /// </summary>
    public string? UserRole =>
        _httpContextAccessor.HttpContext?
            .User?
            .FindFirstValue(ClaimTypes.Role);



    /// <summary>
    /// Gets all roles for the user (in case there are multiple role claims).
    /// </summary>
    public IEnumerable<string> UserRoles =>
        _httpContextAccessor.HttpContext?
            .User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();

    /// <summary>
    /// Checks if the current user has the specified role.
    /// </summary>
    public bool HasRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return UserRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the current user is an admin.
    /// </summary>
    public bool IsAdmin => HasRole("Admin");

    /// <summary>
    /// Indicates if a user is authenticated.
    /// </summary>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;


    public string OrganisationId
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.User == null) return string.Empty;

            // try common claim names used by various identity providers
            var claim = ctx.User.FindFirst("organization_id")
                    ?? ctx.User.FindFirst("Organisation")
                    ?? ctx.User.FindFirst("org")
                    ?? ctx.User.FindFirst("tenant")
                    ?? ctx.User.FindFirst("tenant_id");

            return claim?.Value ?? string.Empty;
        }
    }


}

