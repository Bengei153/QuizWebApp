using System;
using QuizSystem.Api.QuestionSystem.Domain.Enums;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.UpdateFolder;

public class UpdateQuestionCommand
{
    public UpdateQuestionCommand(string Text, QuestionType Type, Guid FolderId, Guid GroupId)
    {
        this.Text = Text;
        this.Type = Type;
        this.FolderId = FolderId;
        this.GroupId = GroupId;
    }

    public Guid Id { get; set; }
    public string Text { get; init; } = null!;
    public QuestionType Type { get; init; }
    public Guid FolderId { get; init; }
    public Guid GroupId { get; init; }
}
