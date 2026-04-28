using MediatR;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class AdminCreateQuestionCommand : IRequest<Guid>
{
    public string Text { get; set; } = null!;
    public string Type { get; set; } = null!;
    public Guid FolderId { get; set; }
    public List<string> Options { get; set; } = new();
}
