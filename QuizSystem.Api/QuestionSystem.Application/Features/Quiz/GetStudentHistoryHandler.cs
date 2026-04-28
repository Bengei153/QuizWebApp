using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetStudentHistoryHandler : IRequestHandler<GetStudentHistoryQuery, List<QuizAttemptHistoryDto>>
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentHistoryHandler(
        IQuizAttemptRepository attemptRepository,
        ICurrentUserService currentUserService)
    {
        _attemptRepository = attemptRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<QuizAttemptHistoryDto>> Handle(
        GetStudentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            throw new UnauthorizedAccessException("User must be authenticated");

        var userId = Guid.Parse(_currentUserService.UserId);
        var attempts = await _attemptRepository.GetByUserAsync(userId);

        return attempts
            .OrderByDescending(a => a.StartedAt)
            .Select(a => new QuizAttemptHistoryDto
            {
                Id = a.Id,
                GroupName = "Quiz",
                Score = a.Score,
                TotalQuestions = a.TotalQuestions,
                IsCompleted = a.isCompleted,
                StartedAt = a.StartedAt,
                SubmittedAt = a.SubmittedAt,
                DurationSeconds = a.SubmittedAt.HasValue
                    ? (int)(a.SubmittedAt.Value - a.StartedAt).TotalSeconds
                    : null
            })
            .ToList();
    }
}
