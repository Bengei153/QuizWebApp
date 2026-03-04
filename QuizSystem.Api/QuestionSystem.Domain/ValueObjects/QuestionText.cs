using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

public record QuestionText
{
    public string Value { get; private set; } = null!;

    private QuestionText() { }

    public QuestionText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Question text cannot be empty");

        Value = value;
    }

    public override string ToString() => Value;

    public QuestionText(QuestionText original)
    {
        Value = original.Value;
    }
}

