using MediatR;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class AdminUpdateQuestionCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<string> Options { get; set; } = new();

    public AdminUpdateQuestionCommand(Guid id, string text, string type, List<string> options)
    {
        Id = id;
        Text = text;
        Type = type;
        Options = options;
    }
}
