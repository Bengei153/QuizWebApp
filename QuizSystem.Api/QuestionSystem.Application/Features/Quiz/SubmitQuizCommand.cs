using System;
using MediatR;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public record SubmitQuizCommand(Guid AttemptId)
    : IRequest<int>;
