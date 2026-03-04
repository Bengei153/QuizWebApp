using System;
using FluentValidation;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Answers;

public sealed class CreateQuestionValidator
    : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty();

        RuleFor(x => x.FolderId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}