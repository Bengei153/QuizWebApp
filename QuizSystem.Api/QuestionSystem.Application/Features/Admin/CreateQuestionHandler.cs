using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Domain.Enums;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class AdminCreateQuestionHandler : IRequestHandler<AdminCreateQuestionCommand, Guid>
{
    private readonly IFolderRepository _folderRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminCreateQuestionHandler(
        IFolderRepository folderRepository,
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _folderRepository = folderRepository;
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        AdminCreateQuestionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            throw new UnauthorizedAccessException("User must be authenticated");

        var folder = await _folderRepository.GetByIdAsync(request.FolderId, Guid.Empty);
        if (folder == null)
            throw new InvalidOperationException("Folder not found");

        var questionType = Enum.Parse<QuestionType>(request.Type);
        var questionText = new QuestionText(request.Text);
        var question = new Question(questionText, questionType, folder.Id)
        {
            CreatedByUserId = _currentUserService.UserId
        };

        foreach (var option in request.Options)
        {
            question.AddOption(option);
        }

        await _questionRepository.AddAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return question.Id;
    }
}
