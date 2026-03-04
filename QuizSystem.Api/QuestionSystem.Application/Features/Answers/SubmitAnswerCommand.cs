using System;

namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public sealed class SubmitAnswerCommand
{
    public Guid QuestionId { get; init; }
    public Guid folderId { get; set; }
    public Guid QuestionGroupId { get; set; }
    public Guid RespondentId { get; init; }

    // For option-based answers (single or multi-select)
    public IReadOnlyCollection<Guid> OptionIds { get; init; }
        = Array.Empty<Guid>();

    // For text answers
    public string? TextValue { get; init; }
}


