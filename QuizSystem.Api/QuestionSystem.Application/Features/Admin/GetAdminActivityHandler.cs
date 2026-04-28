using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class GetAdminActivityHandler : IRequestHandler<GetAdminActivityQuery, List<AdminActivityDto>>
{
    private readonly IQuizAttemptRepository _attemptRepository;

    public GetAdminActivityHandler(IQuizAttemptRepository attemptRepository)
    {
        _attemptRepository = attemptRepository;
    }

    public async Task<List<AdminActivityDto>> Handle(
        GetAdminActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activities = new List<AdminActivityDto>();

        return activities
            .OrderByDescending(a => a.CompletedAt)
            .ToList();
    }
}
