using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Features.Admin;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetAdminStats()
    {
        try
        {
            var stats = await _mediator.Send(new GetAdminStatsQuery());
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetAllQuestions()
    {
        try
        {
            var questions = await _mediator.Send(new GetAllQuestionsQuery());
            return Ok(questions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("questions/create")]
    public async Task<IActionResult> CreateQuestion([FromBody] AdminCreateQuestionCommand command)
    {
        try
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllQuestions), new { id }, new { id });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPut("questions/{id}")]
    public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] AdminUpdateQuestionCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        try
        {
            var command = new AdminDeleteQuestionCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
    {
        try
        {
            var groups = await _mediator.Send(new GetAdminGroupsQuery());
            return Ok(groups);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _mediator.Send(new GetAdminUsersQuery());
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity()
    {
        try
        {
            var activity = await _mediator.Send(new GetAdminActivityQuery());
            return Ok(activity);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("export/pdf")]
    public async Task<IActionResult> ExportToPdf([FromBody] ExportToPdfCommand command)
    {
        try
        {
            var pdfBytes = await _mediator.Send(command);
            if (pdfBytes == null || pdfBytes.Length == 0)
                return NotFound(new { message = "No data to export" });

            return File(pdfBytes, "application/pdf", "quiz_results.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
}
