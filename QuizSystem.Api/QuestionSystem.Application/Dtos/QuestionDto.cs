using QuizSystem.Api.QuestionSystem.Domain.Enums;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public sealed class QuestionDto
{
    public Guid Id { get; init; }
    public QuestionText Text { get; init; } = null!;
    public Guid FolderId { get; init; }
    public QuestionType Type { get; init; }
    public IReadOnlyList<QuestionOptionDto> Options { get; init; } = [];
}

