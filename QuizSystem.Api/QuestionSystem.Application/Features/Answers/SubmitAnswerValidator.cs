using System;
using FluentValidation;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Answers;

public class SubmitAnswerValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.RespondentId).NotEmpty();

        RuleFor(x => x)
            .Must(x => x.OptionIds.Any() || !string.IsNullOrWhiteSpace(x.TextValue))
            .WithMessage("Answer must contain text or at least one option.");
    }
}
