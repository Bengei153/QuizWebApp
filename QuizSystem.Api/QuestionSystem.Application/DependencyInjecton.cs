using System;
using MediatR;
using QuizSystem.Api.QuestionSystem.Application.Features;
using QuizSystem.Api.QuestionSystem.Application.Features.Folders;
using QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.GetQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.UpdateQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Quiz;
using QuizSystem.Api.QuestionSystem.Application.Security;

namespace QuizSystem.Api.QuestionSystem.Application;

/// <summary>
/// Dependency injection configuration for the Application layer.
/// 
/// This registers:
/// - Handler services (business logic)
/// - Command services
/// - Security services (OwnershipGuard)
/// - Authorization helpers
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Register all handlers and security services.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Legacy handlers (to be refactored with ownership checks)
        services.AddScoped<DeleteFolderHandler>();
        services.AddScoped<UpdateFolderHandler>();
        services.AddScoped<GetFolderHandler>();
        services.AddScoped<GetAllFolderHandler>();
        services.AddScoped<CreateFolderHandler>();
        services.AddScoped<CreateQuestionHandler>();
        services.AddScoped<CreateQuestionGroupHandler>();
        services.AddScoped<AddOptionHandler>();
        services.AddScoped<GetQuestionHandler>();
        services.AddScoped<GetQuestionGroupHandler>();
        services.AddScoped<UpdateQuestionGroupHandler>();
        services.AddScoped<SubmitAnswerHandler>();
        services.AddScoped<DeleteQuestionGroupHandler>();
        services.AddScoped<UpdateQuestionHandler>();

        services.AddMediatR(typeof(GetAttemptDetailsHandler).Assembly);
        services.AddMediatR(typeof(GetAttemptDetailsHandler).Assembly);
        services.AddMediatR(typeof(SubmitAnswerHandler).Assembly);
        services.AddMediatR(typeof(SaveAnswerHandler).Assembly);

        // NEW: Enhanced handlers with authorization
        services.AddScoped<UpdateFolderHandlerWithAuth>();
        services.AddScoped<DeleteFolderHandlerWithAuth>();

        // Security & Authorization
        services.AddScoped<OwnershipGuard>();

        return services;
    }

    /// <summary>
    /// Register all command services.
    /// </summary>
    public static IServiceCollection AddApplicationCommand(this IServiceCollection services)
    {
        services.AddScoped<GetQuestionQuery>();

        return services;
    }
}



