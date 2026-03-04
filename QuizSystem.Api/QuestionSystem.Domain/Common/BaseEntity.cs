using System;

namespace QuizSystem.Api.QuestionSystem.Domain.Common;

/// <summary>
/// Base entity with audit trail and soft delete support.
/// All entities inherit these properties for security, compliance, and data recovery.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    
    /// <summary>
    /// UTC timestamp when entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// UTC timestamp when entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag. Entities are never physically deleted from the database.
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// UTC timestamp when entity was soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
