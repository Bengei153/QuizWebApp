using System;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Domain.Entities;

/// <summary>
/// QuestionGroup entity representing a top-level container for quiz questions.
/// 
/// Security:
/// - CreatedByUserId: User who created this group
/// - IsDeleted + DeletedAt: Soft delete support
/// </summary>
public class QuestionGroup : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string OrganisationId { get; set; } = default!;

    private readonly List<Folder> _folders = new();
    public IReadOnlyCollection<Folder> Folders => _folders;

    /// <summary>
    /// User ID who created this question group. Used for ownership authorization.
    /// </summary>
    public string CreatedByUserId { get; set; } = default!;

    private QuestionGroup() { }

    public QuestionGroup(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Group name is required");

        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddFolder(Folder folder)
    {
        if (folder == null)
            throw new ArgumentNullException(nameof(folder));

        _folders.Add(folder);
    }

    /// <summary>
    /// Soft delete the question group.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted question group.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }

    /// <summary>
    /// Update group name with timestamp.
    /// </summary>
    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Group name is required");

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}

