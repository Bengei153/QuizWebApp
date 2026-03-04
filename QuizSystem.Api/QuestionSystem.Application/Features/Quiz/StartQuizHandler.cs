using System;
using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public class StartQuizHandler
    : IRequestHandler<StartQuizCommand, StartQuizDto>
{
    private readonly IFolderRepository _groupRepository;
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly ICurrentUserService _currentUser;

    public StartQuizHandler(
        IFolderRepository groupRepository,
        IQuizAttemptRepository attemptRepository,
        ICurrentUserService currentUser)
    {
        _groupRepository = groupRepository;
        _attemptRepository = attemptRepository;
        _currentUser = currentUser;
    }

    public async Task<StartQuizDto> Handle(
        StartQuizCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository
            .GetByIdAsync(request.folderId, request.groupId);

        if (group == null)
            throw new InvalidOperationException("Quiz not found.");

        if (!group.Questions.Any())
            throw new InvalidOperationException("No questions in this quiz.");

        if (string.IsNullOrEmpty(_currentUser.UserId))
            throw new InvalidOperationException("User must be authenticated to start a quiz.");

        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuestionGroupId = group.Id,
            UserId = Guid.Parse(_currentUser.UserId),
            StartedAt = DateTime.UtcNow,
            isCompleted = false,
            Score = 0,
            Answers = new List<AttemptAnswer>()
        };

        await _attemptRepository.AddAsync(attempt);

        return new StartQuizDto
        {
            AttemptId = attempt.Id,
            FolderName = group.Name,
            Questions = group.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Options = q.Options.Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Text = o.Text
                }).ToList()
            }).ToList()
        };
    }
}