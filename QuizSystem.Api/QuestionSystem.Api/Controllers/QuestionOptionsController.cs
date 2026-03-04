using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Features.Questions.AddOption;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class QuestionOptionsController : ControllerBase
    {
        private readonly AddOptionHandler _handler;

        public QuestionOptionsController(AddOptionHandler handler)
        {
            _handler = handler;
        }

        [HttpPost("{questionId:guid}")]
        public async Task<IActionResult> Add(Guid questionId, string request)
        {
            try
            {
                await _handler.Handle(new AddOptionCommand
                {
                    QuestionId = questionId,
                    Text = request
                });
                return Ok(new { message = "Option added successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [Authorize(Roles = "Admin, Creator")]
        [HttpDelete]
        public IActionResult RemoveOption(Guid questionId, Guid optionId)
        {
            try
            {
                // RemoveQuestionOptionCommand
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
