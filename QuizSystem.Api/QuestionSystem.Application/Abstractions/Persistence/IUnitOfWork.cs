using System;

namespace QuizSystem.Api.QuestionSystem.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}

