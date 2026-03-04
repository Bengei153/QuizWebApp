using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

/// <summary>
/// Enhanced command with CurrentUserContext.
/// 
/// CRITICAL: The CurrentUserContext MUST be populated by the Controller/API layer
/// from JWT claims - NEVER from the request body.
/// 
/// This prevents authorization bypass where a user could spoof another user's ID.
/// </summary>
public sealed record UpdateFolderCommandWithAuth(
    Guid FolderId,
    Guid GroupId,
    string Name,
    CurrentUserContext CurrentUser  // Passed from Controller, extracted from JWT
);
