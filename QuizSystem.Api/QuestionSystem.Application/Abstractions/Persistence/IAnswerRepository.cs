using System;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

public interface IAnswerRepository
{
    Task AddAsync(Answer answer);
}
