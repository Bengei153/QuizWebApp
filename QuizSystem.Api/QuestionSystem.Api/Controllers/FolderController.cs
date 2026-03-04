using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Features.Folders;
using QuizSystem.Api.QuestionSystem.Domain.Common;

namespace QuizSystem.Api.QuestionSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class FolderController : ControllerBase
    {
        private readonly CreateFolderHandler _handler;
        private readonly GetFolderHandler _getFolder;
        private readonly GetAllFolderHandler _getAllFolder;
        private readonly UpdateFolderHandler _updateHandler;
        private readonly DeleteFolderHandler _deleteFolder;
        private readonly ICurrentUserService _currentUserService;

        public FolderController(CreateFolderHandler handler,
            GetFolderHandler getFolder,
            GetAllFolderHandler getAllFolder,
            UpdateFolderHandler updateHandler,
            DeleteFolderHandler deleteFolder,
            ICurrentUserService currentUser)
        {
            _handler = handler;
            _getFolder = getFolder;
            _getAllFolder = getAllFolder;
            _updateHandler = updateHandler;
            _deleteFolder = deleteFolder;
            _currentUserService = currentUser;
        }

        [Authorize(Roles = "Creator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFolderCommand command)
        {
            try
            {
                var userId = _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                    throw new InvalidOperationException("Couldn't get user");

                await _handler.Handle(command);
                return Ok(new { message = "Folder Created successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "AnyRole")]
        [HttpGet("{groupId:guid}/folders/{folderId:guid}")]
        public async Task<IActionResult> Get(Guid folderId, Guid groupId)
        {
            try
            {
                var userId = _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                    throw new InvalidOperationException("Couldn't get user");

                var command = new GetFolderCommand(folderId, groupId);

                var result = await _getFolder.Handle(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "AnyRole")]
        [HttpGet("{groupId:guid}/folders/")]
        public async Task<IActionResult> GetAll(Guid groupId)
        {
            try
            {
                var command = new GetAllFoldersCommand(groupId);

                var result = await _getAllFolder.Handle(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize(Roles = "CreatorOrAdmin")]
        [HttpPut("question-groups/{groupId}/folders/{folderId}")]
        public async Task<IActionResult> UpdateFolder(
            Guid groupId,
            Guid folderId,
            UpdateFolderCommand request)
        {
            try
            {
                var result = await _updateHandler.Handle(new UpdateFolderCommand(
                    groupId,
                    folderId,
                    request.Name
                ));

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

        [Authorize(Roles = "CreatorOrAdmin")]
        [HttpDelete("question-groups/{groupId}/folders/{folderId}")]
        public async Task<IActionResult> DeleteFolder(Guid groupId, Guid folderId)
        {
            try
            {
                await _deleteFolder.Handle(new DeleteFolderCommand(folderId, groupId));
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
