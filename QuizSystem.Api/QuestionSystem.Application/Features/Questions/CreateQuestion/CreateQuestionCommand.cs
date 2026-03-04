using System;
using QuizSystem.Api.QuestionSystem.Domain.Enums;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;

public sealed class CreateQuestionCommand
{
    public CreateQuestionCommand(string Text, QuestionType Type, Guid FolderId, Guid GroupId)
    {
        this.Text = Text;
        this.Type = Type;
        this.FolderId = FolderId;
        this.GroupId = GroupId;
    }

    public string Text { get; init; } = null!;
    public QuestionType Type { get; init; }
    public Guid FolderId { get; init; }
    public Guid GroupId { get; init; }
}

