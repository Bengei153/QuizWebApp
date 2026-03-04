using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Interfaces;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;
using QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

namespace QuizSystem.Api.QuestionSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));


        services.AddScoped<IQuestionGroupRepository, QuestionGroupRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<IOptionRepository, OptionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
