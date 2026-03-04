using System;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid folderId, Guid groupId);
    Task<List<Folder>> GetAllAsync(Guid groupId);
    Task AddAsync(Folder folder);
    Task UpdateAsync(Folder folder);
    Task DeleteAsync(Folder folder);
}
