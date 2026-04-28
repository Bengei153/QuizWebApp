using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Features.Quiz;
using QuizSystem.Api.QuestionSystem.Application.Features.QuestionGroup;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers;

[Route("api/student")]
[ApiController]
[Authorize(Roles = "Viewer")]
public class StudentController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var stats = await _mediator.Send(new GetStudentStatsQuery());
            return Ok(stats);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetQuizHistory()
    {
        try
        {
            var history = await _mediator.Send(new GetStudentHistoryQuery());
            return Ok(history);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("question-groups")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var groups = await _mediator.Send(new GetQuestionGroupsQuery());
            return Ok(groups);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
}
