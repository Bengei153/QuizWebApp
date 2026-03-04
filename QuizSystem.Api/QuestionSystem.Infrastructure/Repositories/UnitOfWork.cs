using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
