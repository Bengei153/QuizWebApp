using System;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Interfaces;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(Guid Id, Guid folderId);
    Task<Question?> GetWithOptionsAsync(Guid id);
    Task AddAsync(Question question);
    Task UpdateAsync(Question question);
    Task GetWithOptionsAsync(object questionId);
    Task<int> CountByGroupIdAsync(Guid groupId);
}