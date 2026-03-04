using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

/// <summary>
/// Delete folder command with soft delete and ownership validation.
/// Includes CurrentUserContext for authorization checks.
/// </summary>
public sealed record DeleteFolderCommandWithAuth(
    Guid FolderId,
    Guid GroupId,
    CurrentUserContext CurrentUser
);
