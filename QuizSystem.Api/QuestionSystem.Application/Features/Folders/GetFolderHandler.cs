using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public class GetFolderHandler
{
    public readonly IFolderRepository folderRepository;
    public readonly IUnitOfWork unitOfWork;

    public GetFolderHandler(IFolderRepository folderRepository, IUnitOfWork unitOfWork)
    {
        this.folderRepository = folderRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<FolderDto?> Handle(GetFolderCommand command)
    {
        var folder = await folderRepository.GetByIdAsync(command.folderId, command.groupId);

        if (folder == null)
            throw new InvalidOperationException("Folder could not be found");

        return new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name
        };
    }
}
