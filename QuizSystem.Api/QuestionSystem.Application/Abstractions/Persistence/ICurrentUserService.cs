namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

/// <summary>
/// Service for accessing the current logged-in user's context.
/// This interface abstracts HttpContext access for better testability.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The current user's ID extracted from JWT 'sub' claim.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// The current user's role from JWT 'role' claim.
    /// </summary>
    string? UserRole { get; }

    /// <summary>
    /// All roles for the current user.
    /// </summary>
    IEnumerable<string> UserRoles { get; }

    /// <summary>
    /// All roles for the current user.
    /// </summary>
    string OrganisationId { get; }

    /// <summary>
    /// Indicates if the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// Indicates if the current user is an Admin.
    /// </summary>
    bool IsAdmin { get; }
}
