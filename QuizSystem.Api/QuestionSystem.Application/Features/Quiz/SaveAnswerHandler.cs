using System;
using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class SaveAnswerHandler : IRequestHandler<SaveAnswerCommand>
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IOptionRepository _optionRepository;

    public SaveAnswerHandler(
        IQuizAttemptRepository attemptRepository,
        IOptionRepository optionRepository)
    {
        _attemptRepository = attemptRepository;
        _optionRepository = optionRepository;
    }

    public async Task<Unit> Handle(
        SaveAnswerCommand command,
        CancellationToken cancellationToken)
    {
        var attempt = await _attemptRepository
            .GetWithAnswersAsync(command.AttemptId);

        if (attempt == null)
            throw new InvalidOperationException("Attempt not found.");

        if (attempt.isCompleted)
            throw new InvalidOperationException("Quiz already completed.");

        var isCorrect = await _optionRepository
            .IsCorrectOptionAsync(command.SelectedOptionId);

        var existingAnswer = attempt.Answers
            .FirstOrDefault(a => a.QuestionId == command.QuestionId);

        if (existingAnswer != null)
        {
            existingAnswer.SelectedOptionId = command.SelectedOptionId;
            existingAnswer.IsCorrect = isCorrect;
        }
        else
        {
            attempt.Answers.Add(new AttemptAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = command.QuestionId,
                SelectedOptionId = command.SelectedOptionId,
                IsCorrect = isCorrect
            });
        }

        await _attemptRepository.UpdateAsync(attempt);

        return Unit.Value;
    }
}
