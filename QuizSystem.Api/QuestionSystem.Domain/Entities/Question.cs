using System;
using System.Text.Json.Serialization;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.Enums;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

/// <summary>
/// Question entity representing a quiz question with optional answers/options.
/// 
/// Security:
/// - CreatedByUserId: User who created this question
/// - IsDeleted + DeletedAt: Soft delete support
/// </summary>
public class Question : BaseEntity
{
    public QuestionText Text { get; private set; } = null!;
    public QuestionType Type { get; private set; }
    public Guid FolderId { get; private set; }

    private readonly List<QuestionOption> _options = new();
    public IReadOnlyCollection<QuestionOption> Options => _options;

    /// <summary>
    /// User ID who created this question. Used for ownership authorization.
    /// </summary>
    public string CreatedByUserId { get; set; } = default!;

    private Question() { }

    public Question(QuestionText text, QuestionType type, Guid folderId)
    {
        Text = text;
        Type = type;
        FolderId = folderId;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddOption(string text)
    {
        if (Type == QuestionType.Text)
            throw new DomainException("Text questions cannot have options");

        _options.Add(new QuestionOption(text));
    }

    /// <summary>
    /// Soft delete the question.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted question.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }

    /// <summary>
    /// Update question text with timestamp.
    /// </summary>
    public void UpdateText(QuestionText text)
    {
        Text = text;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ValidateAnswer(IReadOnlyCollection<AnswerValue> values)
    {
        if (Type == QuestionType.Text)
        {
            if (values.Count != 1 || values.Any(v => v.TextValue is null))
                throw new InvalidOperationException("Text question requires exactly one text answer.");
        }

        if (Type == QuestionType.SingleChoice)
        {
            if (values.Count != 1 || values.Any(v => v.OptionId is null))
                throw new InvalidOperationException("Single choice question requires exactly one option.");
        }

        if (Type == QuestionType.MultipleChoice)
        {
            if (!values.Any() || values.Any(v => v.OptionId is null))
                throw new InvalidOperationException("Multi choice question requires one or more options.");
        }
    }
}


