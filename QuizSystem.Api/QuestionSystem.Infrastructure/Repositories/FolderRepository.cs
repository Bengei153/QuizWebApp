using Microsoft.EntityFrameworkCore;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;
using System;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

public sealed class FolderRepository : IFolderRepository
{
    private readonly AppDbContext _context;

    public FolderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Folder?> GetByIdAsync(Guid folderId, Guid groupId)
        => await _context.Folders
            .Include(f => f.Questions)
            .FirstOrDefaultAsync(f =>
                f.Id == folderId &&
                f.QuestionGroupId == groupId &&
                f.IsDeleted != true);

    public async Task AddAsync(Folder folder)
        => await _context.Folders.AddAsync(folder);

    public async Task<List<Folder>> GetAllAsync(Guid groupId)
    {
        return await _context.Folders
        .Where(f => f.QuestionGroupId == groupId && f.IsDeleted != true)
        .ToListAsync();
    }

    public async Task UpdateAsync(Folder folder)
    {
        _context.Folders.Update(folder);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Folder folder)
    {
        folder.IsDeleted = true;
        folder.DeletedAt = DateTime.UtcNow;

        _context.Folders.Update(folder);
        await _context.SaveChangesAsync();

        var questions = await _context.Questions
            .Where(q => q.FolderId == folder.Id)
            .ToListAsync();

        foreach (var q in questions)
        {
            q.IsDeleted = true;
        }

    }
}


