using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetStudentStatsQuery : IRequest<StudentStatsDto>
{
}
