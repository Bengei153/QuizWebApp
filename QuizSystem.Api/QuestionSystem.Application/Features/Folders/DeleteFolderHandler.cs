using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public class DeleteFolderHandler
{
    private readonly IFolderRepository _folderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteFolderHandler(
        IFolderRepository folderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteFolderCommand command)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Couldn't get user");

        var folder = await _folderRepository.GetByIdAsync(
            command.folderId,
            command.groupId);

        if (folder == null)
            throw new InvalidOperationException("Folder not found in this group");

        // Ownership validation: User must own the folder or be an admin
        if (folder.CreatedByUserId != userId && !_currentUserService.IsAdmin)
            throw new ForbiddenAccessException("You do not have permission to delete this folder");

        folder.SoftDelete();

        await _folderRepository.UpdateAsync(folder);
        await _unitOfWork.SaveChangesAsync();
    }

}
