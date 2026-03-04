namespace QuizSystem.Api.QuestionSystem.Application.Dtos
{
    public sealed class QuestionGroupDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        
        /// <summary>
        /// User ID who created this question group.
        /// </summary>
        public string? CreatedByUserId { get; init; }
        
        /// <summary>
        /// UTC timestamp when question group was created.
        /// </summary>
        public DateTime CreatedAt { get; init; }
        
        /// <summary>
        /// UTC timestamp when question group was last updated (if applicable).
        /// </summary>
        public DateTime? UpdatedAt { get; init; }
        
        /// <summary>
        /// Indicates if question group is soft deleted.
        /// </summary>
        public bool IsDeleted { get; init; }
    }
}
