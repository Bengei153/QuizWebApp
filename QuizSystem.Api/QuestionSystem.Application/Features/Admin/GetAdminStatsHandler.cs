using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class GetAdminStatsHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    private readonly IQuestionGroupRepository _groupRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuizAttemptRepository _attemptRepository;

    public GetAdminStatsHandler(
        IQuestionGroupRepository groupRepository,
        IQuestionRepository questionRepository,
        IQuizAttemptRepository attemptRepository)
    {
        _groupRepository = groupRepository;
        _questionRepository = questionRepository;
        _attemptRepository = attemptRepository;
    }

    public async Task<AdminStatsDto> Handle(
        GetAdminStatsQuery request,
        CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetAllAsync();
        var activeGroups = groups.Where(g => !g.IsDeleted).ToList();
        var totalQuestionGroups = activeGroups.Count;
        var totalQuestions = activeGroups
            .SelectMany(g => g.Folders)
            .Where(f => !f.IsDeleted)
            .SelectMany(f => f.Questions)
            .Count(q => !q.IsDeleted);

        var stats = new AdminStatsDto
        {
            TotalUsers = 0,
            TotalQuestionGroups = totalQuestionGroups,
            TotalQuestions = totalQuestions,
            TotalQuizzesTaken = 0,
            AverageCompletionRate = 0,
            ActiveUsersThisMonth = 0
        };

        return stats;
    }
}
