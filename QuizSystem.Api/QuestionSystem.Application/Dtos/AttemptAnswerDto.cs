using System;

namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

public class AttemptAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
    public bool isCorrect { get; set; }
}
