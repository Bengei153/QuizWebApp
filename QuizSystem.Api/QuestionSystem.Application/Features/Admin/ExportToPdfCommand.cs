using MediatR;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class ExportToPdfCommand : IRequest<byte[]>
{
    public List<Guid>? QuizAttemptIds { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? QuizName { get; set; }
}
