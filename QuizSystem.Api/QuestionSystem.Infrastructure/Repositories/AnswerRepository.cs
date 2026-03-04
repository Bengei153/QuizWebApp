using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

public sealed class AnswerRepository : IAnswerRepository
{
    private readonly AppDbContext _context;

    public AnswerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Answer answer)
        => await _context.Answers.AddAsync(answer);
}
