namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class AdminActivityDto
{
    public Guid Id { get; set; }
    public string UserEmail { get; set; } = null!;
    public string QuizName { get; set; } = null!;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CompletedAt { get; set; }
}
