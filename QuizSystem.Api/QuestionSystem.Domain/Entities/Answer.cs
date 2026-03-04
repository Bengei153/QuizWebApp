using System;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

public class Answer : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public Guid RespondentId { get; private set; }

    private readonly List<AnswerValue> _values = new();
    public IReadOnlyCollection<AnswerValue> Values => _values.AsReadOnly();

    private Answer() { } // EF Core

    public Answer(Guid questionId, Guid respondentId)
    {
        QuestionId = questionId;
        RespondentId = respondentId;
    }

    public void AddText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Text answer cannot be empty.");

        _values.Add(AnswerValue.FromText(value));
    }

    public void AddOption(Guid optionId)
    {
        _values.Add(AnswerValue.FromOption(optionId));
    }
}
