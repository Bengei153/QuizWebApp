using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Admin;

public class GetAdminUsersHandler : IRequestHandler<GetAdminUsersQuery, List<AdminUserDto>>
{
    private readonly IQuizAttemptRepository _attemptRepository;

    public GetAdminUsersHandler(IQuizAttemptRepository attemptRepository)
    {
        _attemptRepository = attemptRepository;
    }

    public async Task<List<AdminUserDto>> Handle(
        GetAdminUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = new Dictionary<string, AdminUserDto>();

        return users.Values.ToList();
    }
}
