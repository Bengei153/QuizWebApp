using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence
{
    public interface IQuestionGroupRepository
    {
        Task<QuestionGroup?> GetByIdAsync(Guid id, string organisationId);
        Task<IEnumerable<QuestionGroup>> GetAllByOrganisationAsync(string organisationId);
        Task AddAsync(QuestionGroup questionGroup);
        Task UpdateAsync(QuestionGroup questionGroup);
        Task<bool> DeleteAsync(Guid id, string organisationId);
    }
}
