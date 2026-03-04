using System;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

/// <summary>
/// Folder entity representing a collection of questions within a QuestionGroup.
/// 
/// Security:
/// - CreatedByUserId: User who created this folder
/// - IsDeleted + DeletedAt: Soft delete support for data recovery
/// </summary>
public class Folder : BaseEntity
{
    public string Name { get; set; } = null!;
    public Guid QuestionGroupId { get; private set; }

    private readonly List<Question> _questions = new();
    public IReadOnlyCollection<Question> Questions => _questions;

    /// <summary>
    /// User ID who created this folder. Used for ownership authorization.
    /// </summary>
    public string CreatedByUserId { get; set; } = default!;

    private Folder() { }

    public Folder(string name, Guid groupId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Folder name is required");

        Name = name;
        QuestionGroupId = groupId;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddQuestion(Question question)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));

        _questions.Add(question);
    }

    /// <summary>
    /// Soft delete the folder.
    /// Physical deletion is never used - all data is preserved for compliance.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted folder.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }

    /// <summary>
    /// Update folder name with timestamp.
    /// </summary>
    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Folder name is required");

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}
