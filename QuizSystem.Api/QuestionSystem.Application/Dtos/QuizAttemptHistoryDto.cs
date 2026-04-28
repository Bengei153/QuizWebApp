namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class QuizAttemptHistoryDto
{
    public Guid Id { get; set; }
    public string GroupName { get; set; } = null!;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? DurationSeconds { get; set; }
}
