namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public sealed record UpdateFolderCommand(
    Guid folderId,
    Guid groupId,
    string Name
    );
