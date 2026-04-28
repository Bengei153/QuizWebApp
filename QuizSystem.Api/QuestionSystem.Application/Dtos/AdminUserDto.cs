namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class AdminUserDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public int QuizzesTaken { get; set; }
    public double AverageScore { get; set; }
    public DateTime LastActivityAt { get; set; }
}
