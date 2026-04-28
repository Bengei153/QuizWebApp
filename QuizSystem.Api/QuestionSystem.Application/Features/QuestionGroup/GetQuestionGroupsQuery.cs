using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

public class GetQuestionGroupsQuery : IRequest<List<QuestionGroupListDto>>
{
}
