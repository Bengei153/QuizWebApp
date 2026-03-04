using System;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public sealed class CreateFolderHandler
{
    private readonly IFolderRepository _folderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateFolderHandler(
        IFolderRepository folderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateFolderCommand command)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Couldn't get user");

        var folder = new Folder(
            command.Name,
            command.GroupId)
        {
            CreatedByUserId = userId
        };

        await _folderRepository.AddAsync(folder);
        await _unitOfWork.SaveChangesAsync();

        return folder.Id;
    }

}
