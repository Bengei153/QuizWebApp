using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Security;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

public sealed class DeleteQuestionGroupHandler
{
    private readonly IQuestionGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OwnershipGuard _ownershipGuard;

    public DeleteQuestionGroupHandler(
        IQuestionGroupRepository repository,
        IUnitOfWork unitOfWork,
        OwnershipGuard ownershipGuard)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _ownershipGuard = ownershipGuard;
    }

    public async Task Handle(DeleteQuestionGroupCommand command)
    {
        // Validation: Ensure user ID exists (extracted from JWT)
        var userId = command.CurrentUser.UserId;
        var orgId = command.CurrentUser.OrganisationId;
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(orgId))
            throw new ForbiddenAccessException("User/Organisation identification failed");

        var questionGroup = await _repository.GetByIdAsync(command.Id, orgId);

        if (questionGroup == null)
            throw new InvalidOperationException("Question Group not found");

        // AUTHORIZATION CHECK: User must own the question group or be Admin
        _ownershipGuard.ValidateOwnership(questionGroup.CreatedByUserId, "QuestionGroup");

        // Use soft delete to preserve data for compliance and audit trails
        questionGroup.SoftDelete();

        await _repository.UpdateAsync(questionGroup);
        await _unitOfWork.SaveChangesAsync();
    }
}

