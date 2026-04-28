namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalQuestionGroups { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalQuizzesTaken { get; set; }
    public double AverageCompletionRate { get; set; }
    public int ActiveUsersThisMonth { get; set; }
}
