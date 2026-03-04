using System;

namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

public interface IOptionRepository
{
    Task<bool> IsCorrectOptionAsync(Guid optionId);
}

