using System;
using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public record GetAttemptDetailsQuery(Guid attemptId) : IRequest<QuizAttemptDetailsDto>;
