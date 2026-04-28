using MediatR;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class ExportToPdfHandler : IRequestHandler<ExportToPdfCommand, byte[]>
{
    public async Task<byte[]> Handle(
        ExportToPdfCommand request,
        CancellationToken cancellationToken)
    {
        var pdfBytes = new byte[0];

        return pdfBytes;
    }
}
