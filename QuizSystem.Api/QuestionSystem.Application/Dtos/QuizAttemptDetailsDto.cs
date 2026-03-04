using System;

namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class QuizAttemptDetailsDto
{
    public Guid Id { get; set; }
    public int Score { get; set; }
    public bool isCompleted { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<AttemptAnswerDto>? Answers { get; set; }

}
