using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Interfaces;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features;

public sealed class SubmitAnswerHandler
{
    private readonly IAnswerRepository _answerRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitAnswerHandler(
        IAnswerRepository answerRepository,
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork)
    {
        _answerRepository = answerRepository;
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SubmitAnswerCommand command)
    {
        // Load question (needed for validation)
        var question = await _questionRepository.GetByIdAsync(command.QuestionId, command.folderId);
        if (question is null)
            throw new InvalidOperationException("Question not found.");

        // Build answer (YOUR logic preserved)
        var answer = new Answer(
            command.QuestionId,
            command.RespondentId);

        if (command.OptionIds.Any())
        {
            foreach (var optionId in command.OptionIds)
            {
                answer.AddOption(optionId);
            }
        }
        else if (!string.IsNullOrWhiteSpace(command.TextValue))
        {
            answer.AddText(command.TextValue);
        }
        else
        {
            throw new InvalidOperationException(
                "Either option(s) or text answer must be provided.");
        }

        // Domain validation (single source of truth)
        question.ValidateAnswer(answer.Values);

        // Persist
        await _answerRepository.AddAsync(answer);
        await _unitOfWork.SaveChangesAsync();
    }
}
