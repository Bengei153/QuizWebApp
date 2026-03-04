using System;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

public class QuizAttempt
{
    public Guid Id { get; set; }
    public Guid QuestionGroupId { get; set; }
    public Guid UserId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool isCompleted { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<AttemptAnswer> Answers { get; set; } = new();
}
