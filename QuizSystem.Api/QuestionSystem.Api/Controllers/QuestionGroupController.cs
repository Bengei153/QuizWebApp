using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;
using QuizSystem.Api.QuestionSystem.Domain.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QuizSystem.Api.QuestionSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuestionGroupController : ControllerBase
    {

        private readonly CreateQuestionGroupHandler _CreateHandler;
        private readonly GetQuestionGroupHandler _getHandler;
        private readonly UpdateQuestionGroupHandler _updateHandler;
        private readonly DeleteQuestionGroupHandler _deleteHandler;
        private readonly ICurrentUserService _currentUserService;
        public QuestionGroupController(
            GetQuestionGroupHandler getQuestionHandler,
            CreateQuestionGroupHandler handler,
            UpdateQuestionGroupHandler updateHandler,
            DeleteQuestionGroupHandler deleteHandler,
            ICurrentUserService currentUserService)
        {
            _getHandler = getQuestionHandler;
            _CreateHandler = handler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
            _currentUserService = currentUserService;
        }

        // GET api/<QuestionGroupController>/5
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Creator")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var orgId = _currentUserService.OrganisationId;

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(orgId))
                    return Unauthorized(new { message = "User identification failed" });

                var userContext = new CurrentUserContext { UserId = userId, Role = _currentUserService.UserRole, OrganisationId = orgId };
                var command = new GetQuestionGroupCommand(id, userContext);
                var result = await _getHandler.HandleGroup(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        // POST api/<QuestionGroupController>
        [HttpPost]
        [Authorize(Roles = "Creator, Admin")]
        public async Task<IActionResult> Create([FromBody] CreateQuestionGroupCommand command)
        {
            try
            {
                // Extract authenticated user from JWT claims
                var userId = _currentUserService.UserId;
                var orgId = _currentUserService.OrganisationId;

                if (string.IsNullOrWhiteSpace(userId))
                    throw new InvalidOperationException("User not found");

                await _CreateHandler.Handle(command);


                return Ok(new { message = "Question Group created successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        // PUT api/<QuestionGroupController>/5
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "CreatorOrAdmin")]
        public async Task<IActionResult> Put(Guid id, [FromBody] string name)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var userRole = _currentUserService.UserRole;

                if (string.IsNullOrWhiteSpace(userId))
                    throw new InvalidOperationException();

                var userContext = new CurrentUserContext
                {
                    UserId = userId,
                    Role = userRole
                };

                var command = new UpdateQuestionGroupCommand(id, name, userContext);

                var result = await _updateHandler.Handle(command);

                return Ok(result);
            }
            catch (ForbiddenAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        // DELETE api/<QuestionGroupController>/5
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "CreatorOrAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var userRole = _currentUserService.UserRole;

                if (string.IsNullOrWhiteSpace(userId))
                    throw new InvalidOperationException();

                var userContext = new CurrentUserContext
                {
                    UserId = userId,
                    Role = userRole
                };

                var command = new DeleteQuestionGroupCommand(id, userContext);

                await _deleteHandler.Handle(command);

                return NoContent();
            }
            catch (ForbiddenAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

    }
}
