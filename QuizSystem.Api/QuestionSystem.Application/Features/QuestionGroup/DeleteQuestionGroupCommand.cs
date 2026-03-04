using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

public sealed class DeleteQuestionGroupCommand
{
    public Guid Id;
    public CurrentUserContext CurrentUser { get; set; }

    public DeleteQuestionGroupCommand(Guid id, CurrentUserContext CurrentUser)
    {
        Id = id;
        this.CurrentUser = CurrentUser;
    }
}
