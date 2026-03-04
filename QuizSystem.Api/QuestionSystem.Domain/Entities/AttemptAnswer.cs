using System;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

public class AttemptAnswer
{
    public Guid Id { get; set; }
    public Guid QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = null!;
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
}
