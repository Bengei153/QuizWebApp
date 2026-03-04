namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public sealed class AnswerDto
{
    public Guid QuestionId { get; init; }
    public Guid RespondentId { get; init; }
    public string Value { get; init; } = null!;
}