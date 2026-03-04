using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup
{
    public class GetQuestionGroupHandler
    {
        private readonly IQuestionGroupRepository _repository;

        public GetQuestionGroupHandler(IQuestionGroupRepository repository)
        {
            _repository = repository;
        }

        public async Task<QuestionGroupDto?> HandleGroup(GetQuestionGroupCommand command)
        {
            // Validation: Ensure user ID exists (extracted from JWT)
            var userId = command.User.UserId;
            var orgId = command.User.OrganisationId;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(orgId))
                throw new ForbiddenAccessException("User/Organisation identification failed");

            var questionGroup = await _repository.GetByIdAsync(command.Id, orgId);

            if (questionGroup is null || questionGroup.IsDeleted)
                throw new InvalidOperationException("No Group Id Found");

            // Allow Admin to bypass org restriction
            if (!string.Equals(command.User.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(questionGroup.OrganisationId) ||
                    !string.Equals(questionGroup.OrganisationId, command.User.OrganisationId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ForbiddenAccessException("Access to this organisation's resource is forbidden");
                }
            }

            return new QuestionGroupDto
            {
                Id = questionGroup.Id,
                Name = questionGroup.Name
            };


        }
    }
}
