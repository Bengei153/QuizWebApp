using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Security;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

/// <summary>
/// Enhanced Update Handler with comprehensive authorization.
/// 
/// Security flow:
/// 1. Validates user is authenticated
/// 2. Checks if user owns the folder OR is Admin
/// 3. Throws ForbiddenAccessException (403) if unauthorized
/// 4. Updates folder with new timestamp
/// 
/// Pattern: This handler demonstrates best practices for all update operations.
/// Follow this same pattern for Questions and QuestionGroups.
/// </summary>
public class UpdateFolderHandlerWithAuth
{
    private readonly IFolderRepository _folderRepository;
    private readonly OwnershipGuard _ownershipGuard;

    public UpdateFolderHandlerWithAuth(
        IFolderRepository folderRepository,
        OwnershipGuard ownershipGuard)
    {
        _folderRepository = folderRepository;
        _ownershipGuard = ownershipGuard;
    }

    public async Task<FolderDto> Handle(UpdateFolderCommandWithAuth command)
    {
        // Validation: Ensure user ID exists (extracted from JWT)
        var userId = command.CurrentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new ForbiddenAccessException("User authentication failed");

        // Get the resource
        var folder = await _folderRepository.GetByIdAsync(command.GroupId, command.FolderId);
        if (folder == null)
            throw new InvalidOperationException("Folder not found in this group");

        // AUTHORIZATION CHECK: User must own the folder or be Admin
        // This is the CRITICAL security check - all update operations must include this
        _ownershipGuard.ValidateOwnership(folder.CreatedByUserId, "Folder");

        // Business logic: Update folder
        folder.Update(command.Name);

        // Persistence
        await _folderRepository.UpdateAsync(folder);

        // Response DTO
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
