using MediatR;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class AdminDeleteQuestionCommand : IRequest<Unit>
{
    public Guid Id { get; set; }

    public AdminDeleteQuestionCommand(Guid id)
    {
        Id = id;
    }
}
