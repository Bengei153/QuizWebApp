using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Interfaces;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;

public sealed class AddOptionHandler
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddOptionHandler(
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddOptionCommand command)
    {
        var question = await _questionRepository
            .GetWithOptionsAsync(command.QuestionId);

        if (question is null)
            throw new InvalidOperationException("Question not found");

        question.AddOption(command.Text);

        await _unitOfWork.SaveChangesAsync();
    }
}
