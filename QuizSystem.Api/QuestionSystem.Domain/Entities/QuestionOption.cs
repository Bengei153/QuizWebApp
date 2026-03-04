using System;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

public class QuestionOption : BaseEntity
{
    public string Text { get; private set; } = null!;
    public bool isCorrect { get; set; }

    private QuestionOption() { }

    public QuestionOption(string text)
    {
        Text = text;
    }
}

