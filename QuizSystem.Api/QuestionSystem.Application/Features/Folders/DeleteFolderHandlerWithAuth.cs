using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Security;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

/// <summary>
/// Enhanced Delete Handler with soft delete and ownership validation.
/// 
/// Security flow:
/// 1. Validates user is authenticated
/// 2. Checks if user owns the folder OR is Admin
/// 3. Performs soft delete (sets IsDeleted=true, DeletedAt=UTC now)
/// 4. Throws ForbiddenAccessException (403) if unauthorized
/// 
/// IMPORTANT: Physical deletion is NEVER performed.
/// All data is preserved for:
/// - Data recovery
/// - Audit trails
/// - Compliance requirements
/// - Forensics
/// </summary>
public class DeleteFolderHandlerWithAuth
{
    private readonly IFolderRepository _folderRepository;
    private readonly OwnershipGuard _ownershipGuard;

    public DeleteFolderHandlerWithAuth(
        IFolderRepository folderRepository,
        OwnershipGuard ownershipGuard)
    {
        _folderRepository = folderRepository;
        _ownershipGuard = ownershipGuard;
    }

    public async Task Handle(DeleteFolderCommandWithAuth command)
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
        _ownershipGuard.ValidateOwnership(folder.CreatedByUserId, "Folder");

        // Business logic: Soft delete (preserves data)
        folder.SoftDelete();

        // Persistence
        await _folderRepository.UpdateAsync(folder);
    }
}
