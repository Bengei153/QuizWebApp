namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class QuestionDetailDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string FolderName { get; set; } = null!;
    public List<QuestionOptionDto> Options { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; } = null!;
}
