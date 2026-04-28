using System;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(Guid Id, Guid folderId);
    Task<Question?> GetWithOptionsAsync(Guid id);
    Task AddAsync(Question question);
    Task UpdateAsync(Question question);
    Task<int> CountByGroupIdAsync(Guid groupId);
}
