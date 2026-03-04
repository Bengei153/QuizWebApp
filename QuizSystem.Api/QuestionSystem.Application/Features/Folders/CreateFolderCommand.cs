using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public sealed class CreateFolderCommand
{
    public Guid GroupId { get; init; }
    public string Name { get; init; } = null!;
    public string? userId { get; set; }

    public CreateFolderCommand(Guid GroupId, string Name, string userId)
    {
        this.Name = Name;
        this.GroupId = GroupId;
        this.userId = userId;
    }
}
