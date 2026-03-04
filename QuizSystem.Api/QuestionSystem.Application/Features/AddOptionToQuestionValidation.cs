using System;
using FluentValidation;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions;

public sealed class AddOptionToQuestionValidator
    : AbstractValidator<AddOptionCommand>
{
    public AddOptionToQuestionValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(200);
    }
}
