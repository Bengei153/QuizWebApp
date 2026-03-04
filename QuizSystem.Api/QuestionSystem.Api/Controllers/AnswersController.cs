using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Features;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly SubmitAnswerHandler _handler;

        public AnswersController(SubmitAnswerHandler handler)
        {
            _handler = handler;
        }

        [Authorize(Roles = "Creator")]
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitAnswerCommand command)
        {
            try
            {
                await _handler.Handle(command);
                return Ok(new { message = "Answer submitted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
    }
}
