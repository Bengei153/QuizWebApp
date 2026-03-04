using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

namespace QuizSystem.Api.QuestionSystem.Application.Security;

/// <summary>
/// Centralized authorization guard for ownership-based access control.
/// 
/// CRITICAL: All ownership checks must go through this class to prevent authorization bypass.
/// 
/// Authorization rules:
/// - Admins always have access (bypass ownership checks)
/// - Non-admins can only access resources they created
/// 
/// This ensures:
/// - No duplicate ownership logic across handlers
/// - Consistent authorization across the application
/// - Easy testing of authorization rules
/// - Compliance with security requirements
/// </summary>
public class OwnershipGuard
{
    private readonly ICurrentUserService _currentUserService;

    public OwnershipGuard(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Validates that the current user owns the resource or is an Admin.
    /// Throws ForbiddenAccessException (403) if unauthorized.
    /// </summary>
    /// <param name="resourceOwnerId">The user ID that created the resource</param>
    /// <param name="resourceName">Name of the resource for error message (e.g., "Question", "Folder")</param>
    /// <exception cref="ForbiddenAccessException">If user doesn't own the resource and isn't Admin</exception>
    public void ValidateOwnership(string resourceOwnerId, string resourceName = "resource")
    {
        var userId = _currentUserService.UserId;

        // Security check: User must be authenticated
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ForbiddenAccessException($"Authentication required to access {resourceName}");
        }

        // Authorization check: User is owner OR is Admin
        bool isOwner = userId.Equals(resourceOwnerId, StringComparison.OrdinalIgnoreCase);
        bool isAdmin = _currentUserService.IsAdmin;

        if (!isOwner && !isAdmin)
        {
            throw new ForbiddenAccessException(
                $"You do not have permission to modify this {resourceName}. Only the creator or admins can make changes.");
        }
    }

    /// <summary>
    /// Validates that the current user owns the resource OR is allowed by the provided predicate.
    /// Useful for complex authorization scenarios.
    /// </summary>
    public void ValidateOwnershipOrCondition(string resourceOwnerId, Func<bool> additionalCondition, string resourceName = "resource")
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ForbiddenAccessException($"Authentication required to access {resourceName}");
        }

        bool isOwner = userId.Equals(resourceOwnerId, StringComparison.OrdinalIgnoreCase);
        bool isAdmin = _currentUserService.IsAdmin;
        bool additionalAuth = additionalCondition?.Invoke() ?? false;

        if (!isOwner && !isAdmin && !additionalAuth)
        {
            throw new ForbiddenAccessException(
                $"You do not have permission to modify this {resourceName}.");
        }
    }

    /// <summary>
    /// Get the current user's ID and throw if not authenticated.
    /// </summary>
    public string GetCurrentUserIdOrThrow()
    {
        var userId = _currentUserService.UserId;
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ForbiddenAccessException("Authentication required");
        }

        return userId;
    }

    /// <summary>
    /// Check if the current user is an Admin.
    /// Useful for admin-only operations.
    /// </summary>
    public bool IsCurrentUserAdmin() => _currentUserService.IsAdmin;

    /// <summary>
    /// Check if the current user owns a resource.
    /// Does NOT throw - returns boolean for conditional logic.
    /// </summary>
    public bool IsCurrentUserOwner(string resourceOwnerId)
    {
        var userId = _currentUserService.UserId;
        
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return userId.Equals(resourceOwnerId, StringComparison.OrdinalIgnoreCase);
    }
}
