namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public class GetAllFoldersCommand
{
    public GetAllFoldersCommand(Guid groupId)
    {
        this.groupId = groupId;
    }

    public Guid groupId { get; set; }
}
