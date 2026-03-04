using System;

namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class QuizAttemptDto
{
    public Guid Id { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
