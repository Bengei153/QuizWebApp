using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Features.Quiz;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizAttemptController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuizAttemptController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Creator")]
        [HttpPost("start/{folderId}")]
        public async Task<IActionResult> StartQuiz(Guid folderId, Guid groupId)
        {
            try
            {
                var attemptId = await _mediator.Send(new StartQuizCommand(folderId, groupId));
                return Ok(new { attemptId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "Creator")]
        [HttpPost("submit/{attemptId}")]
        public async Task<IActionResult> Submit(Guid attemptId)
        {
            try
            {
                var score = await _mediator.Send(new SubmitQuizCommand(attemptId));
                return Ok(new { score });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "Creator")]
        [HttpGet("my-attempts")]
        public async Task<IActionResult> GetMyAttempts(Guid attemptId)
        {
            try
            {
                var attempts = await _mediator.Send(new GetAttemptDetailsQuery(attemptId));
                return Ok(attempts);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "Creator")]
        [HttpGet("{attemptId}")]
        public async Task<IActionResult> GetAttempt(Guid attemptId)
        {
            try
            {
                var attempt = await _mediator.Send(new GetAttemptDetailsQuery(attemptId));
                return Ok(attempt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
    }
}
