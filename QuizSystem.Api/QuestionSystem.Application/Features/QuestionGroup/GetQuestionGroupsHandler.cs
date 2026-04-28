using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

public class GetQuestionGroupsHandler : IRequestHandler<GetQuestionGroupsQuery, List<QuestionGroupListDto>>
{
    private readonly IQuestionGroupRepository _groupRepository;

    public GetQuestionGroupsHandler(IQuestionGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<List<QuestionGroupListDto>> Handle(
        GetQuestionGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetAllAsync();

        return groups
            .Where(g => !g.IsDeleted)
            .Select(g => new QuestionGroupListDto
            {
                Id = g.Id,
                Name = g.Name,
                FolderCount = g.Folders.Count(f => !f.IsDeleted),
                TotalQuestions = g.Folders
                    .Where(f => !f.IsDeleted)
                    .Sum(f => f.Questions.Count(q => !q.IsDeleted)),
                CreatedAt = g.CreatedAt
            })
            .OrderByDescending(g => g.CreatedAt)
            .ToList();
    }
}
