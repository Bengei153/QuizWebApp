using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class GetAdminActivityQuery : IRequest<List<AdminActivityDto>>
{
}
