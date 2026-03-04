using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

public sealed class GetQuestionGroupCommand
{
    public Guid Id { get; }
    public CurrentUserContext User { get; }

    public GetQuestionGroupCommand(Guid id, CurrentUserContext user)
    {
        Id = id;
        User = user;
    }
}
