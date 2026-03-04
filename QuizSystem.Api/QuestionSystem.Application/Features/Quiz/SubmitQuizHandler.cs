using System;
using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class SubmitQuizHandler
    : IRequestHandler<SubmitQuizCommand, int>
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IOptionRepository _optionRepository;
    private readonly ICurrentUserService _currentUser;

    public SubmitQuizHandler(
        IQuizAttemptRepository attemptRepository,
        IOptionRepository optionRepository,
        ICurrentUserService currentUser)
    {
        _attemptRepository = attemptRepository;
        _optionRepository = optionRepository;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(
        SubmitQuizCommand command,
        CancellationToken cancellationToken)
    {
        var attempt = await _attemptRepository
            .GetWithAnswersAsync(command.AttemptId);

        if (attempt == null)
            throw new InvalidOperationException("Attempt not found.");

        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            throw new Exception("User not logged in");

        var userId = Guid.Parse(_currentUser.UserId);

        // Ownership rule
        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("Not your attempt.");

        if (attempt.isCompleted)
            throw new InvalidOperationException("Quiz already submitted.");

        int score = 0;

        foreach (var answer in attempt.Answers)
        {
            var isCorrect = await _optionRepository
                .IsCorrectOptionAsync(answer.SelectedOptionId);

            answer.IsCorrect = isCorrect;

            if (isCorrect)
                score++;
        }

        attempt.Score = score;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.isCompleted = true;

        await _attemptRepository.UpdateAsync(attempt);

        return score;
    }
}
