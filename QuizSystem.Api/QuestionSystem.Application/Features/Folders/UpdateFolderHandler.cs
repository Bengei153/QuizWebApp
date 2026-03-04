using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public class UpdateFolderHandler
{
    private readonly IFolderRepository _folderRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateFolderHandler(
        IFolderRepository folderRepository,
        ICurrentUserService currentUserService)
    {
        _folderRepository = folderRepository;
        _currentUserService = currentUserService;
    }

    public async Task<FolderDto> Handle(UpdateFolderCommand command)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Couldn't get user");

        var folder = await _folderRepository.GetByIdAsync(command.groupId, command.folderId);

        if (folder == null)
            throw new InvalidOperationException("Folder not found in this group");

        // Ownership validation: User must own the folder or be an admin
        if (folder.CreatedByUserId != userId && !_currentUserService.IsAdmin)
            throw new ForbiddenAccessException("You do not have permission to modify this folder");

        folder.Update(command.Name);

        await _folderRepository.UpdateAsync(folder);

        return new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            CreatedByUserId = folder.CreatedByUserId,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt,
            IsDeleted = folder.IsDeleted
        };
    }

}
