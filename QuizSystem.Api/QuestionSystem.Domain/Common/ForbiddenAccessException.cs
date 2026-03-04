namespace QuizSystem.Api.QuestionSystem.Domain.Common;

/// <summary>
/// Thrown when a user attempts to access a resource they don't own (403 Forbidden).
/// Used for ownership-based authorization checks.
/// </summary>
public class ForbiddenAccessException : DomainException
{
    public ForbiddenAccessException(string message = "You do not have permission to access this resource.")
        : base(message)
    {
    }
}
