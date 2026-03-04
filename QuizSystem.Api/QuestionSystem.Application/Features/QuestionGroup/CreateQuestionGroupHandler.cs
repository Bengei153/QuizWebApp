using QuizSystem.Api.QuestionSystem.Application.Abstractions;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Features.Folders;

namespace QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup
{
    public class CreateQuestionGroupHandler
    {
        private readonly IQuestionGroupRepository _questionGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateQuestionGroupHandler(
            IQuestionGroupRepository questionGroupRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _questionGroupRepository = questionGroupRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateQuestionGroupCommand command)
        {
            var userId = _currentUserService.UserId;
            var orgId = _currentUserService.OrganisationId;
            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("Couldn't get user");

            var questionGroup = new Domain.Entities.QuestionGroup(
                command.Name)
            {
                CreatedByUserId = userId,
                OrganisationId = orgId
            };

            await _questionGroupRepository.AddAsync(questionGroup);
            await _unitOfWork.SaveChangesAsync();

            return questionGroup.Id;
        }
    }
}
