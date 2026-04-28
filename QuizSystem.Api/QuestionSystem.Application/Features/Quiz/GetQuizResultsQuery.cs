using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetQuizResultsQuery : IRequest<QuizResultsDto>
{
    public Guid AttemptId { get; set; }

    public GetQuizResultsQuery(Guid attemptId)
    {
        AttemptId = attemptId;
    }
}
