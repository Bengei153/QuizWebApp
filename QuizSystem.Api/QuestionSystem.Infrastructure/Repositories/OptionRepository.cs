using System;
using Microsoft.EntityFrameworkCore;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

public class OptionRepository : IOptionRepository
{
    private readonly AppDbContext _context;

    public OptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsCorrectOptionAsync(Guid optionId)
    {
        var option = await _context.QuestionOptions
                                            .Where(o => o.Id == optionId)
                                            .FirstOrDefaultAsync();

        if (option == null)
            throw new InvalidOperationException("Option Not Found");

        return option.isCorrect;
    }
}
