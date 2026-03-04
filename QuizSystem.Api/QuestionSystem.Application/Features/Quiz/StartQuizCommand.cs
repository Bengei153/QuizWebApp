using QuizSystem.Api.QuestionSystem.Application.Dtos;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

public record StartQuizCommand(Guid folderId, Guid groupId)
    : MediatR.IRequest<StartQuizDto>;