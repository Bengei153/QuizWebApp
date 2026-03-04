using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;

public sealed class AddOptionCommand
{
    public Guid QuestionId { get; init; }
    public string Text { get; init; } = null!;
}
