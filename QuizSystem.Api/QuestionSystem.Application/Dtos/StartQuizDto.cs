using System;

namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class StartQuizDto
{
    public Guid AttemptId { get; set; }
    public string FolderName { get; set; } = null!;
    public List<QuestionDto> Questions { get; set; } = null!;
}