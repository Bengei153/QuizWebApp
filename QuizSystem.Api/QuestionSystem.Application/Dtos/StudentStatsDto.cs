namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class StudentStatsDto
{
    public int TotalQuizzesTaken { get; set; }
    public double AverageScore { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalAnswers { get; set; }
    public double PassRate { get; set; }
    public int QuizzesInProgress { get; set; }
}
