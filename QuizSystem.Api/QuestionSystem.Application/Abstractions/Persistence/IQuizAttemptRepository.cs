using System;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

public interface IQuizAttemptRepository
{
    Task AddAsync(QuizAttempt attempt);
    Task<QuizAttempt?> GetByIdAsync(Guid id);
    Task<List<QuizAttempt>> GetByUserAsync(Guid userId);
    Task<QuizAttempt?> GetWithAnswersAsync(Guid attemptId);
    Task UpdateAsync(QuizAttempt attempt);

}
