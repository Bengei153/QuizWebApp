namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public class GetFolderCommand
{
    public GetFolderCommand(Guid folderId, Guid groupId)
    {
        this.folderId = folderId;
        this.groupId = groupId;
    }

    public Guid folderId { get; set; }
    public Guid groupId { get; set; }
}
