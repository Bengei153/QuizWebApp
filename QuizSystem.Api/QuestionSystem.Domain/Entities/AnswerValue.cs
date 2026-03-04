using System;

namespace QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

public class AnswerValue
{
    public string TextValue { get; private set; } = null!;
    public Guid? OptionId { get; private set; }

    private AnswerValue() { }

    public static AnswerValue FromText(string value)
        => new AnswerValue { TextValue = value };

    public static AnswerValue FromOption(Guid optionId)
        => new AnswerValue { OptionId = optionId };
}


