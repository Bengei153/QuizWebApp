using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Interfaces;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;

public sealed class CreateQuestionHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateQuestionHandler(
        IQuestionRepository questionRepository,
        IFolderRepository folderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CreateQuestionCommand command)
    {
        //Ensure user is logged in
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("User is not authorized");
        // Ensure folder exists
        var folder = await _folderRepository.GetByIdAsync(command.FolderId, command.GroupId);
        if (folder is null)
            throw new InvalidOperationException("Folder does not exist.");

        var commandText = new QuestionText(command.Text);

        var question = new Question(
            commandText,
            command.Type,
            command.FolderId
        )
        {
            CreatedByUserId = userId
        };

        await _questionRepository.AddAsync(question);
        await _unitOfWork.SaveChangesAsync();
    }
}