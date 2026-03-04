using System;
using Microsoft.EntityFrameworkCore;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Interfaces;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

public sealed class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _context;

    public QuestionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Question?> GetByIdAsync(Guid Id, Guid folderId)
        => await _context.Questions.FirstOrDefaultAsync(q =>
                                                    q.Id == Id &&
                                                    q.FolderId == folderId);

    public async Task<Question?> GetWithOptionsAsync(Guid id)
        => await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

    public async Task AddAsync(Question question)
        => await _context.Questions.AddAsync(question);

    public Task GetWithOptionsAsync(object questionId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountByGroupIdAsync(Guid groupId)
    {
        return await _context.Questions.CountAsync(q => q.FolderId == groupId);
    }
}