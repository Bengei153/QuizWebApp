using System;
using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class GetAttemptDetailsHandler : IRequestHandler<GetAttemptDetailsQuery, QuizAttemptDetailsDto>
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly ICurrentUserService _currentUser;

    public GetAttemptDetailsHandler(IQuizAttemptRepository attemptRepository, ICurrentUserService currentUser)
    {
        _attemptRepository = attemptRepository;
        _currentUser = currentUser;
    }

    public async Task<QuizAttemptDetailsDto> Handle(GetAttemptDetailsQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _attemptRepository.GetWithAnswersAsync(request.attemptId);

        if (attempt == null)
            throw new InvalidOperationException("Attempt not found");

        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            throw new ForbiddenAccessException("You are not authorzed to view this file");

        var userId = Guid.Parse(_currentUser.UserId);

        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("Not ur attempt");

        return new QuizAttemptDetailsDto
        {
            Id = attempt.Id,
            Score = attempt.Score,
            isCompleted = attempt.isCompleted,
            SubmittedAt = attempt.SubmittedAt,
            Answers = attempt.Answers.Select(a => new AttemptAnswerDto
            {
                QuestionId = a.QuestionId,
                OptionId = a.SelectedOptionId,
                isCorrect = a.IsCorrect
            }).ToList()
        };
    }
}
