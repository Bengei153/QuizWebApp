using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Security;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

public class UpdateQuestionGroupHandler
{
    private readonly IQuestionGroupRepository _questionGroupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OwnershipGuard _ownershipGuard;

    public UpdateQuestionGroupHandler(
        IQuestionGroupRepository questionGroupRepository,
        IUnitOfWork unitOfWork,
        OwnershipGuard ownershipGuard)
    {
        _questionGroupRepository = questionGroupRepository;
        _unitOfWork = unitOfWork;
        _ownershipGuard = ownershipGuard;
    }

    public async Task<QuestionGroupDto?> Handle(UpdateQuestionGroupCommand command)
    {
        // Validation: Ensure user ID exists (extracted from JWT)
        var userId = command.CurrentUser.UserId;
        var orgId = command.CurrentUser.OrganisationId;
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(orgId))
            throw new ForbiddenAccessException("User/Organisation identification failed");

        var questionGroup = await _questionGroupRepository.GetByIdAsync(command.Id, orgId);

        if (questionGroup == null)
            throw new InvalidOperationException("Question Group not found");

        // AUTHORIZATION CHECK: User must own the question group or be Admin
        _ownershipGuard.ValidateOwnership(questionGroup.CreatedByUserId, "QuestionGroup");

        questionGroup.Update(command.Name);

        await _questionGroupRepository.UpdateAsync(questionGroup);
        await _unitOfWork.SaveChangesAsync();

        return new QuestionGroupDto
        {
            Id = questionGroup.Id,
            Name = questionGroup.Name,
            CreatedByUserId = questionGroup.CreatedByUserId,
            CreatedAt = questionGroup.CreatedAt,
            UpdatedAt = questionGroup.UpdatedAt,
            IsDeleted = questionGroup.IsDeleted
        };
    }
}
