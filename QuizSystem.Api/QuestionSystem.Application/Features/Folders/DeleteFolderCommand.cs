namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public sealed record DeleteFolderCommand(
    Guid groupId,
    Guid folderId
    );
