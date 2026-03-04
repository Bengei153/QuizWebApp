using System;
using Microsoft.EntityFrameworkCore;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

public class QuizAttemptRepository : IQuizAttemptRepository
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public QuizAttemptRepository(AppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task AddAsync(QuizAttempt attempt)
    {
        await _context.QuizAttempts.AddAsync(attempt);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<QuizAttempt?> GetByIdAsync(Guid id)
    {
        return await _context.QuizAttempts
                                    .Include(a => a.Answers)
                                    .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<QuizAttempt>> GetByUserAsync(Guid userId)
    {
        return await _context.QuizAttempts
                                    .Where(a => a.UserId == userId)
                                    .OrderByDescending(a => a.StartedAt)
                                    .ToListAsync();
    }

    public async Task<QuizAttempt?> GetWithAnswersAsync(Guid attemptId)
    {
        return await _context.QuizAttempts.Include(a => a.Answers)
                                           .FirstOrDefaultAsync(a => a.Id == attemptId);
    }

    public async Task UpdateAsync(QuizAttempt attempt)
    {
        _context.QuizAttempts.Update(attempt);
        await _context.SaveChangesAsync();
    }
}
