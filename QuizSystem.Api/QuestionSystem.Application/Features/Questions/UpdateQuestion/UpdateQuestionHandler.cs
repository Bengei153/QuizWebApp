using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.UpdateFolder;
using QuizSystem.Api.QuestionSystem.Application.Interfaces;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.UpdateQuestion;

public class UpdateQuestionHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuestionHandler(
        IQuestionRepository folderRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = folderRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<QuestionDto?> Handle(UpdateQuestionCommand command)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Couldn't get user");

        var question = await _questionRepository.GetByIdAsync(command.Id, command.FolderId);

        if (question == null)
            throw new InvalidOperationException("Folder not found in this group");

        // Ownership validation: User must own the folder or be an admin
        if (question.CreatedByUserId != userId && !_currentUserService.IsAdmin)
            throw new ForbiddenAccessException("You do not have permission to modify this folder");

        var commandText = new QuestionText(command.Text);

        question.UpdateText(commandText);

        await _unitOfWork.SaveChangesAsync();

        return new QuestionDto
        {
            Id = question.Id,
            Text = commandText,
            Type = question.Type,
        };
    }
}
