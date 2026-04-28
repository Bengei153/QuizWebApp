using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetQuizResultsHandler : IRequestHandler<GetQuizResultsQuery, QuizResultsDto>
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetQuizResultsHandler(
        IQuizAttemptRepository attemptRepository,
        ICurrentUserService currentUserService)
    {
        _attemptRepository = attemptRepository;
        _currentUserService = currentUserService;
    }

    public async Task<QuizResultsDto> Handle(
        GetQuizResultsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            throw new UnauthorizedAccessException("User must be authenticated");

        var attempt = await _attemptRepository.GetWithAnswersAsync(request.AttemptId);
        if (attempt == null)
            throw new InvalidOperationException("Quiz attempt not found");

        var userId = Guid.Parse(_currentUserService.UserId);
        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("Cannot access this quiz result");

        var percentage = attempt.TotalQuestions > 0
            ? (double)attempt.Score / attempt.TotalQuestions * 100
            : 0;

        var passPercentage = 50;
        var passed = percentage >= passPercentage;

        return new QuizResultsDto
        {
            AttemptId = attempt.Id,
            GroupName = "Quiz",
            Score = attempt.Score,
            TotalQuestions = attempt.TotalQuestions,
            Percentage = Math.Round(percentage, 2),
            Passed = passed,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            Questions = attempt.Answers.Select(a => new QuestionResultDto
            {
                QuestionId = Guid.NewGuid(),
                QuestionText = "Question",
                IsCorrect = a.IsCorrect,
                SelectedAnswer = a.SelectedOptionId.ToString(),
                CorrectAnswer = ""
            }).ToList()
        };
    }
}
