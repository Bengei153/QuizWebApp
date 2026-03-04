using System;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestion;


public sealed class GetQuestionQuery
{
    public Guid QuestionId { get; init; }
}