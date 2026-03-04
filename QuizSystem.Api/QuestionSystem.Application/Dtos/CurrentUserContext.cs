namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

/// <summary>
/// Encapsulates the current user's context for passing through the application layer.
/// 
/// CRITICAL: Always extract this from JWT claims in the API layer, never accept UserId from request body.
/// 
/// This ensures:
/// - User identity cannot be spoofed by sending a different UserId in the request
/// - Handlers don't need access to HttpContext (better testability)
/// - User context flows through the CQRS pipeline safely
/// </summary>
public class CurrentUserContext
{
    /// <summary>
    /// User ID extracted from JWT 'sub' claim.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's role extracted from JWT 'role' claim.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Indicates if user is an Admin.
    /// </summary>
    public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>
    /// Indicates if user is a Creator.
    /// </summary>
    public bool IsCreator => Role?.Equals("Creator", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>
    /// Indicates if user is a Viewer.
    /// </summary>
    public bool IsViewer => Role?.Equals("Viewer", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>
    /// Check if user has a specific role.
    /// </summary>
    public bool HasRole(string role) => Role?.Equals(role, StringComparison.OrdinalIgnoreCase) ?? false;

    public string OrganisationId { get; set; } = string.Empty;
}
