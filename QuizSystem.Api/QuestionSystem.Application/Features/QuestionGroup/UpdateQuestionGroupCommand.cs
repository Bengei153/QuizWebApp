using Microsoft.Identity.Client;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

public sealed class UpdateQuestionGroupCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public CurrentUserContext CurrentUser { get; set; }

    public UpdateQuestionGroupCommand(Guid Id, string Name, CurrentUserContext CurrentUser)
    {
        this.Id = Id;
        this.Name = Name;
        this.CurrentUser = CurrentUser;
    }

}
