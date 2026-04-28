namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class QuestionGroupListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int FolderCount { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime CreatedAt { get; set; }
}
