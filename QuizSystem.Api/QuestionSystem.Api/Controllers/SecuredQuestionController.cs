using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers;

/// <summary>
/// Secured Question API Controller demonstrating authorization patterns.
/// 
/// ROLES:
/// - Admin: Full access to all questions
/// - Creator: Can create, read, update own questions
/// - Viewer: Read-only access to questions
/// 
/// Authorization is enforced in the handlers, not just on the controller level.
/// This allows for fine-grained, testable authorization logic.
/// </summary>
[Route("api/folders/{folderId:guid}/questions")]
[ApiController]
[Authorize]  // All endpoints require authentication
public class SecuredQuestionController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public SecuredQuestionController(
        ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Example: Create a question.
    /// Only Creator and Admin roles can create.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateQuestion(
        Guid folderId,
        [FromBody] CreateQuestionRequestDto request)
    {
        try
        {
            // Extract user from JWT claims
            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "User identification failed" });

            // PATTERN: Pass user ID to handler, NOT from request body
            // This prevents UserId spoofing

            // TODO: Create and call handler with CurrentUserContext

            return CreatedAtAction(nameof(GetQuestion),
                new { folderId },
                null);
        }
        catch (ForbiddenAccessException)
        {
            Console.WriteLine("Forbidden access.");
            return Forbid();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Example: Get a question.
    /// All authenticated users (any role) can read.
    /// </summary>
    [HttpGet("{questionId:guid}")]
    [Authorize(Roles = "Admin, Creator, Viewer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuestion(Guid folderId, Guid questionId)
    {
        try
        {
            // TODO: Implement handler
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Example: Update a question.
    /// User must own the question OR be Admin.
    /// </summary>
    [HttpPut("{questionId:guid}")]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(
        Guid folderId,
        Guid questionId,
        [FromBody] UpdateQuestionRequestDto request)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var userRole = _currentUserService.UserRole;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            // PATTERN: Create user context from JWT claims
            var userContext = new CurrentUserContext
            {
                UserId = userId,
                Role = userRole
            };

            // Handler validates ownership + executes update
            // If not authorized: ForbiddenAccessException -> 403

            // TODO: Call handler

            return Ok();
        }
        catch (ForbiddenAccessException)
        {
            Console.WriteLine("Forbidden access.");
            return Forbid();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Example: Delete a question (soft delete).
    /// User must own the question OR be Admin.
    /// </summary>
    [HttpDelete("{questionId:guid}")]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(Guid folderId, Guid questionId)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var userRole = _currentUserService.UserRole;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var userContext = new CurrentUserContext
            {
                UserId = userId,
                Role = userRole
            };

            // Handler validates ownership + performs soft delete

            // TODO: Call handler

            return NoContent();  // 204
        }
        catch (ForbiddenAccessException)
        {
            Console.WriteLine("Forbidden access.");
            return Forbid();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}

/// <summary>
/// Request model for creating a question.
/// </summary>
public class CreateQuestionRequestDto
{
    public string Text { get; set; } = string.Empty;
    // TODO: Add other properties
}

/// <summary>
/// Request model for updating a question.
/// </summary>
public class UpdateQuestionRequestDto
{
    public string Text { get; set; } = string.Empty;
}
