namespace QuizSystem.Api.QuestionSystem.Application.Dtos;

/// <summary>
/// Data Transfer Object for Folder.
/// Includes audit and soft delete information for security and compliance.
/// </summary>
public sealed class FolderDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    
    /// <summary>
    /// User ID who created this folder.
    /// </summary>
    public string? CreatedByUserId { get; init; }
    
    /// <summary>
    /// UTC timestamp when folder was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// UTC timestamp when folder was last updated (if applicable).
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
    
    /// <summary>
    /// Indicates if folder is soft deleted.
    /// </summary>
    public bool IsDeleted { get; init; }
}


