using System;
using MediatR;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public record SaveAnswerCommand(
    Guid AttemptId,
    Guid QuestionId,
    Guid SelectedOptionId
) : IRequest;
