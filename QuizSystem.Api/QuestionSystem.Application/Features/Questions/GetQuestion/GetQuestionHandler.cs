using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Interfaces;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestion;

public sealed class GetQuestionHandler
{
    private readonly IQuestionRepository _repository;

    public GetQuestionHandler(IQuestionRepository repository)
    {
        _repository = repository;
    }

    public async Task<QuestionDto?> Handle(GetQuestionQuery query)
    {
        var question = await _repository.GetWithOptionsAsync(query.QuestionId);
        if (question is null) return null;

        var questionText = new Domain.ValueObjects.QuestionText(question.Text.Value);

        return new QuestionDto
        {
            Id = question.Id,
            Text = questionText,
            Type = question.Type,
            Options = question.Options
                .Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Text = o.Text
                })
                .ToList()
        };
    }
}
