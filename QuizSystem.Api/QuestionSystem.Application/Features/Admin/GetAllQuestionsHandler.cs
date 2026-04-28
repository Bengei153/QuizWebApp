using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class GetAllQuestionsHandler : IRequestHandler<GetAllQuestionsQuery, List<QuestionDetailDto>>
{
    private readonly IQuestionGroupRepository _groupRepository;

    public GetAllQuestionsHandler(IQuestionGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<List<QuestionDetailDto>> Handle(
        GetAllQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetAllAsync();
        var questions = new List<QuestionDetailDto>();

        foreach (var group in groups.Where(g => !g.IsDeleted))
        {
            foreach (var folder in group.Folders.Where(f => !f.IsDeleted))
            {
                foreach (var question in folder.Questions.Where(q => !q.IsDeleted))
                {
                    questions.Add(new QuestionDetailDto
                    {
                        Id = question.Id,
                        Text = question.Text.Value,
                        Type = question.Type.ToString(),
                        FolderName = folder.Name,
                        Options = question.Options
                            .Select(o => new QuestionOptionDto
                            {
                                Id = o.Id,
                                Text = o.Text
                            })
                            .ToList(),
                        CreatedAt = question.CreatedAt,
                        CreatedByUserId = question.CreatedByUserId
                    });
                }
            }
        }

        return questions.OrderByDescending(q => q.CreatedAt).ToList();
    }
}
