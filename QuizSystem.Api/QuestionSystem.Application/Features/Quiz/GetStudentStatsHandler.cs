using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetStudentStatsHandler : IRequestHandler<GetStudentStatsQuery, StudentStatsDto>
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentStatsHandler(
        IQuizAttemptRepository attemptRepository,
        ICurrentUserService currentUserService)
    {
        _attemptRepository = attemptRepository;
        _currentUserService = currentUserService;
    }

    public async Task<StudentStatsDto> Handle(
        GetStudentStatsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            throw new UnauthorizedAccessException("User must be authenticated");

        var userId = Guid.Parse(_currentUserService.UserId);
        var attempts = await _attemptRepository.GetByUserAsync(userId);

        var completedAttempts = attempts.Where(a => a.isCompleted).ToList();
        var totalQuestions = completedAttempts.Sum(a => a.TotalQuestions);
        var correctAnswers = completedAttempts.Sum(a => a.Score);
        var totalAnswers = totalQuestions;
        var inProgressAttempts = attempts.Count(a => !a.isCompleted);

        var averageScore = completedAttempts.Any() 
            ? completedAttempts.Average(a => a.Score) 
            : 0;

        var passRate = totalAnswers > 0 
            ? (double)correctAnswers / totalAnswers * 100 
            : 0;

        return new StudentStatsDto
        {
            TotalQuizzesTaken = completedAttempts.Count,
            AverageScore = Math.Round(averageScore, 2),
            CorrectAnswers = correctAnswers,
            TotalAnswers = totalAnswers,
            PassRate = Math.Round(passRate, 2),
            QuizzesInProgress = inProgressAttempts
        };
    }
}
