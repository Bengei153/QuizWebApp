using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Features.Folders;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers;

/// <summary>
/// Secured Folder API Controller demonstrating best practices for authorization.
/// 
/// SECURITY PATTERNS DEMONSTRATED:
/// 1. [Authorize] - Requires valid JWT token
/// 2. [Authorize(Roles = "...")] - Role-based access control
/// 3. User context extraction from JWT claims (never request body)
/// 4. Ownership validation in handlers
/// 5. Soft delete instead of hard delete
/// 6. 403 Forbidden for unauthorized access
/// 
/// ERROR RESPONSES:
/// - 401 Unauthorized: No valid JWT token
/// - 403 Forbidden: User doesn't own resource and isn't Admin
/// - 400 Bad Request: Invalid input
/// - 404 Not Found: Resource doesn't exist
/// - 500 Internal Server Error: Unexpected errors (handled by middleware)
/// </summary>
[Route("api/question-groups/{groupId:guid}/folders")]
[ApiController]
[Authorize]  // All endpoints require authentication
public class SecuredFolderController : ControllerBase
{
    private readonly UpdateFolderHandlerWithAuth _updateHandler;
    private readonly DeleteFolderHandlerWithAuth _deleteHandler;
    private readonly GetFolderHandler _getFolder;
    private readonly CreateFolderHandler _createFolder;
    private readonly ICurrentUserService _currentUserService;

    public SecuredFolderController(
        UpdateFolderHandlerWithAuth updateHandler,
        DeleteFolderHandlerWithAuth deleteHandler,
        GetFolderHandler getFolder,
        CreateFolderHandler createFolder,
        ICurrentUserService currentUserService)
    {
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getFolder = getFolder;
        _createFolder = createFolder;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get a specific folder. Viewers can read.
    /// </summary>
    /// <remarks>
    /// Authorization: Any authenticated user (Admin, Creator, Viewer)
    /// Returns: 200 OK with folder details, 404 if not found, 401 if not authenticated
    /// </remarks>
    [HttpGet("{folderId:guid}")]
    [Authorize(Roles = "Admin, Creator, Viewer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FolderDto>> GetFolder(Guid groupId, Guid folderId)
    {
        try
        {
            var command = new GetFolderCommand(folderId, groupId);
            var result = await _getFolder.Handle(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Create a new folder. Only Creators and Admins can create.
    /// </summary>
    /// <remarks>
    /// Authorization: Admin, Creator only
    /// 
    /// SECURITY NOTES:
    /// - User ID is extracted from JWT 'sub' claim, NOT from request
    /// - User cannot specify CreatedByUserId themselves
    /// - Prevents authorization bypass
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FolderDto>> CreateFolder(
        Guid groupId,
        [FromBody] CreateFolderRequestDto request)
    {
        try
        {
            // Extract authenticated user from JWT claims
            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "User identification failed" });

            var command = new CreateFolderCommand(groupId, request.Name, userId);
            var result = await _createFolder.Handle(command);

            return CreatedAtAction(nameof(GetFolder), 
                new { groupId, folderId = result }, 
                result);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Update a folder. User must own it or be Admin.
    /// </summary>
    /// <remarks>
    /// Authorization: Must be creator or Admin
    /// Returns: 200 OK on success, 403 Forbidden if not authorized
    /// 
    /// SECURITY FLOW:
    /// 1. User ID extracted from JWT claims
    /// 2. Handler fetches folder from database
    /// 3. OwnershipGuard validates: CreatedByUserId == UserId OR IsAdmin
    /// 4. If validation fails: 403 Forbidden
    /// 5. If valid: Update folder and return 200 OK
    /// </remarks>
    [HttpPut("{folderId:guid}")]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FolderDto>> UpdateFolder(
        Guid groupId,
        Guid folderId,
        [FromBody] UpdateFolderRequestDto request)
    {
        try
        {
            // Extract authenticated user from JWT claims
            var userId = _currentUserService.UserId;
            var userRole = _currentUserService.UserRole;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "User identification failed" });

            // Create user context from claims
            var userContext = new CurrentUserContext
            {
                UserId = userId,
                Role = userRole
            };

            // Execute handler with authorization
            var command = new UpdateFolderCommandWithAuth(folderId, groupId, request.Name, userContext);
            var result = await _updateHandler.Handle(command);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete a folder (soft delete). User must own it or be Admin.
    /// </summary>
    /// <remarks>
    /// Authorization: Must be creator or Admin
    /// Returns: 204 No Content on success, 403 Forbidden if not authorized
    /// 
    /// DELETION POLICY:
    /// - Physical deletion is NEVER performed
    /// - Data is soft deleted: IsDeleted = true, DeletedAt = UTC now
    /// - Original data remains for:
    ///   * Compliance and audit trails
    ///   * Data recovery if needed
    ///   * Forensic investigation
    /// 
    /// DATABASE FILTERING:
    /// - Queries should filter: WHERE IsDeleted = 0
    /// - Deleted items are hidden from normal views
    /// - Admins can still access with separate endpoint if needed
    /// </remarks>
    [HttpDelete("{folderId:guid}")]
    [Authorize(Roles = "Admin, Creator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFolder(Guid groupId, Guid folderId)
    {
        try
        {
            // Extract authenticated user from JWT claims
            var userId = _currentUserService.UserId;
            var userRole = _currentUserService.UserRole;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "User identification failed" });

            // Create user context from claims
            var userContext = new CurrentUserContext
            {
                UserId = userId,
                Role = userRole
            };

            // Execute handler with authorization
            var command = new DeleteFolderCommandWithAuth(folderId, groupId, userContext);
            await _deleteHandler.Handle(command);

            return NoContent();  // 204
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}

/// <summary>
/// Request model for creating a folder.
/// </summary>
public class CreateFolderRequestDto
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating a folder.
/// </summary>
public class UpdateFolderRequestDto
{
    public string Name { get; set; } = string.Empty;
}
