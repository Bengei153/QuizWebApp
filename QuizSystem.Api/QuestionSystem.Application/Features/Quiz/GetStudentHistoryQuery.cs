using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetStudentHistoryQuery : IRequest<List<QuizAttemptHistoryDto>>
{
}
