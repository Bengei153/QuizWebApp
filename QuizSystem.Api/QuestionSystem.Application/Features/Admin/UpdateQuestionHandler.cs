using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class AdminUpdateQuestionHandler : IRequestHandler<AdminUpdateQuestionCommand, Unit>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminUpdateQuestionHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        AdminUpdateQuestionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.UserId))
            throw new UnauthorizedAccessException("User must be authenticated");

        var question = await _questionRepository.GetWithOptionsAsync(request.Id);
        if (question == null)
            throw new InvalidOperationException("Question not found");

        if (question.CreatedByUserId != _currentUserService.UserId && !_currentUserService.IsAdmin)
            throw new UnauthorizedAccessException("Cannot update this question");

        var newText = new QuestionText(request.Text);
        question.UpdateText(newText);

        await _questionRepository.UpdateAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}
