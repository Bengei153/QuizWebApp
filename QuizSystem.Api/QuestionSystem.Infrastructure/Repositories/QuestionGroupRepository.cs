using Microsoft.EntityFrameworkCore;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories
{
    public class QuestionGroupRepository : IQuestionGroupRepository
    {

        private readonly AppDbContext _context;

        public QuestionGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(QuestionGroup questionGroup)
        {
            await _context.QuestionGroups.AddAsync(questionGroup);
        }

        public async Task<bool> DeleteAsync(Guid id, string organisationId)
        {
            var entity = await GetByIdAsync(id, organisationId);
            if (entity == null) return false;

            entity.SoftDelete();
            _context.QuestionGroups.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<QuestionGroup?> GetByIdAsync(Guid id, string organisationId)
        {
            return await _context.QuestionGroups
                .FirstOrDefaultAsync(qg => qg.Id == id
                    && qg.OrganisationId == organisationId
                    && !qg.IsDeleted);
        }

        public async Task<IEnumerable<QuestionGroup>> GetAllByOrganisationAsync(string organisationId)
        {
            return await _context.QuestionGroups
                .Where(qg => qg.OrganisationId == organisationId && !qg.IsDeleted)
                .ToListAsync();
        }

        public async Task UpdateAsync(QuestionGroup questionGroup)
        {
            _context.QuestionGroups.Update(questionGroup);
            await _context.SaveChangesAsync();
        }


    }
}
