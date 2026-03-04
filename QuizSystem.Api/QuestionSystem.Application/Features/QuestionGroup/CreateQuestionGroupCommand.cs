using System.ComponentModel.DataAnnotations;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup
{
    public sealed class CreateQuestionGroupCommand
    {
        public string Name { get; init; } = null!;

    }
}
