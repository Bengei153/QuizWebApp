using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Folders;

public class GetAllFolderHandler
{
    public readonly IFolderRepository folderRepository;
    public readonly IUnitOfWork unitOfWork;

    public GetAllFolderHandler(IFolderRepository folderRepository, IUnitOfWork unitOfWork)
    {
        this.folderRepository = folderRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<List<FolderDto>> Handle(GetAllFoldersCommand command)
    {
        var folder = await folderRepository.GetAllAsync(command.groupId);

        if (folder == null)
            throw new InvalidOperationException("Folder could not be found");

        return folder.Select(f => new FolderDto
        {
            Id = f.Id,
            Name = f.Name
        }).ToList();
    }
}
