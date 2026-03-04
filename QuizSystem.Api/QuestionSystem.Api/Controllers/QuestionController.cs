using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Features;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.CreateQuestion;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.UpdateFolder;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.UpdateQuestion;
using QuizSystem.Api.QuestionSystem.Domain.Enums;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly CreateQuestionHandler _handler;
        private readonly SubmitAnswerHandler _sHandler;
        private readonly UpdateQuestionHandler _updateHandler;

        public QuestionController(CreateQuestionHandler handler, SubmitAnswerHandler sHandler, UpdateQuestionHandler updateHandler)
        {
            _handler = handler;
            _sHandler = sHandler;
            _updateHandler = updateHandler;
        }

        [Authorize(Roles = "Creator")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateQuestionCommand command)
        {
            try
            {
                await _handler.Handle(command);
                return Ok(new { message = "Question created successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "Creator")]
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateQuestionCommand command)
        {
            try
            {
                await _updateHandler.Handle(command);
                return Ok(new { message = "Updated Successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "AnyRole")]
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitAnswerCommand command)
        {
            try
            {
                await _sHandler.Handle(command);
                return Ok(new { message = "Answer submitted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "AnyRole")]
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            try
            {
                // Call GetQuestionByIdQuery handler
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "CreatorOrAdmin")]
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                // Call DeleteQuestionCommand handler
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

    }
}
