/*
╔══════════════════════════════════════════════════════════════════════════════╗
║                    QUICK REFERENCE: CODE EXAMPLES                            ║
║                 Common Patterns & Copy-Paste Ready Solutions                  ║
╚══════════════════════════════════════════════════════════════════════════════╝

This file contains ready-to-use code examples for implementing the authorization
patterns. Copy and adapt these examples for your specific entities.


╔══════════════════════════════════════════════════════════════════════════════╗
║ PATTERN 1: Create Secured Handler                                            ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

// ============================================================================
// EXAMPLE 1A: UpdateQuestionHandlerWithAuth (Secured Update Handler)
// ============================================================================

/*
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;
using QuizSystem.Api.QuestionSystem.Application.Dtos;
using QuizSystem.Api.QuestionSystem.Application.Security;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.Entities;
using QuizSystem.Api.QuestionSystem.Domain.ValueObjects;

namespace QuizSystem.Api.QuestionSystem.Application.Features.Questions;

/// <summary>
/// Update handler for Questions with ownership authorization.
/// 
/// SECURITY: User must own the question OR be Admin
/// </summary>
public class UpdateQuestionHandlerWithAuth
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly OwnershipGuard _ownershipGuard;

    public UpdateQuestionHandlerWithAuth(
        IQuestionRepository questionRepository,
        IFolderRepository folderRepository,
        OwnershipGuard ownershipGuard)
    {
        _questionRepository = questionRepository;
        _folderRepository = folderRepository;
        _ownershipGuard = ownershipGuard;
    }

    public async Task<QuestionDto> Handle(UpdateQuestionCommandWithAuth command)
    {
        // Validation: User must be authenticated
        var userId = command.CurrentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new ForbiddenAccessException("User authentication failed");

        // Get the question
        var question = await _questionRepository.GetByIdAsync(command.QuestionId);
        if (question == null)
            throw new InvalidOperationException("Question not found");

        // CRITICAL AUTHORIZATION CHECK
        _ownershipGuard.ValidateOwnership(question.CreatedByUserId, "Question");

        // Update business logic
        question.UpdateText(new QuestionText(command.Text));

        // Persist
        await _questionRepository.UpdateAsync(question);

        // Return DTO
        return new QuestionDto
        {
            Id = question.Id,
            Text = question.Text.Value,
            CreatedByUserId = question.CreatedByUserId,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            IsDeleted = question.IsDeleted
        };
    }
}

// Command definition
public sealed record UpdateQuestionCommandWithAuth(
    Guid QuestionId,
    string Text,
    CurrentUserContext CurrentUser
);
*/


// ============================================================================
// EXAMPLE 1B: DeleteQuestionHandlerWithAuth (Secured Delete Handler)
// ============================================================================

/*
public class DeleteQuestionHandlerWithAuth
{
    private readonly IQuestionRepository _questionRepository;
    private readonly OwnershipGuard _ownershipGuard;

    public DeleteQuestionHandlerWithAuth(
        IQuestionRepository questionRepository,
        OwnershipGuard ownershipGuard)
    {
        _questionRepository = questionRepository;
        _ownershipGuard = ownershipGuard;
    }

    public async Task Handle(DeleteQuestionCommandWithAuth command)
    {
        var userId = command.CurrentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new ForbiddenAccessException("User authentication failed");

        var question = await _questionRepository.GetByIdAsync(command.QuestionId);
        if (question == null)
            throw new InvalidOperationException("Question not found");

        // CRITICAL: Check ownership before deleting
        _ownershipGuard.ValidateOwnership(question.CreatedByUserId, "Question");

        // Soft delete
        question.SoftDelete();

        // Persist
        await _questionRepository.UpdateAsync(question);
    }
}

public sealed record DeleteQuestionCommandWithAuth(
    Guid QuestionId,
    CurrentUserContext CurrentUser
);
*/


/*
╔══════════════════════════════════════════════════════════════════════════════╗
║ PATTERN 2: Secured Controller Endpoints                                      ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

// ============================================================================
// EXAMPLE 2A: Controller Update Endpoint
// ============================================================================

/*
[HttpPut("{questionId:guid}")]
[Authorize(Roles = "Admin, Creator")]
public async Task<ActionResult<QuestionDto>> UpdateQuestion(
    Guid folderId,
    Guid questionId,
    [FromBody] UpdateQuestionRequestDto request)
{
    try
    {
        // 1. Extract user from JWT claims
        var userId = _currentUserService.UserId;
        var userRole = _currentUserService.UserRole;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "User identification failed" });

        // 2. Create user context
        var userContext = new CurrentUserContext
        {
            UserId = userId,
            Role = userRole
        };

        // 3. Create command with user context
        var command = new UpdateQuestionCommandWithAuth(questionId, request.Text, userContext);

        // 4. Execute handler (throws ForbiddenAccessException if unauthorized)
        var result = await _handler.Handle(command);

        // 5. Return result
        return Ok(result);
    }
    catch (ForbiddenAccessException ex)
    {
        // Authorization failed: User doesn't own resource and isn't admin
        return Forbid();  // Returns 403 Forbidden
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
*/


// ============================================================================
// EXAMPLE 2B: Controller Delete Endpoint
// ============================================================================

/*
[HttpDelete("{questionId:guid}")]
[Authorize(Roles = "Admin, Creator")]
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

        var command = new DeleteQuestionCommandWithAuth(questionId, userContext);
        await _handler.Handle(command);

        return NoContent();  // 204 No Content
    }
    catch (ForbiddenAccessException)
    {
        return Forbid();  // 403
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { message = ex.Message });
    }
}
*/


/*
╔══════════════════════════════════════════════════════════════════════════════╗
║ PATTERN 3: Repository Soft Delete Filtering                                 ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

// ============================================================================
// EXAMPLE 3A: Base Repository Extension
// ============================================================================

/*
namespace QuizSystem.Api.QuestionSystem.Infrastructure.Repositories;

public static class RepositoryExtensions
{
    /// <summary>
    /// Filter out soft-deleted entities.
    /// ALWAYS use this when querying.
    /// </summary>
    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> query)
        where T : BaseEntity
    {
        return query.Where(x => !x.IsDeleted);
    }

    /// <summary>
    /// Get only soft-deleted entities (for admin views).
    /// </summary>
    public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query)
        where T : BaseEntity
    {
        return query.Where(x => x.IsDeleted);
    }
}
*/


// ============================================================================
// EXAMPLE 3B: Using Extension in Repository
// ============================================================================

/*
public class FolderRepository : IFolderRepository
{
    private readonly AppDbContext _context;

    public async Task<Folder?> GetByIdAsync(Guid groupId, Guid folderId)
    {
        // GOOD: Filters soft-deleted
        return await _context.Folders
            .NotDeleted()
            .Where(f => f.Id == folderId && f.QuestionGroupId == groupId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Folder>> GetAllByGroupAsync(Guid groupId)
    {
        // GOOD: Filters soft-deleted
        return await _context.Folders
            .NotDeleted()
            .Where(f => f.QuestionGroupId == groupId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Folder>> GetDeletedFoldersAsync(Guid groupId)
    {
        // For admin view of deleted items
        return await _context.Folders
            .OnlyDeleted()
            .Where(f => f.QuestionGroupId == groupId)
            .ToListAsync();
    }
}
*/


/*
╔══════════════════════════════════════════════════════════════════════════════╗
║ PATTERN 4: Unit Tests                                                        ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

// ============================================================================
// EXAMPLE 4A: Test Ownership Validation
// ============================================================================

/*
[TestFixture]
public class UpdateQuestionHandlerWithAuthTests
{
    private Mock<IQuestionRepository> _mockQuestionRepo;
    private Mock<IFolderRepository> _mockFolderRepo;
    private Mock<OwnershipGuard> _mockOwnershipGuard;
    private UpdateQuestionHandlerWithAuth _handler;

    [SetUp]
    public void Setup()
    {
        _mockQuestionRepo = new Mock<IQuestionRepository>();
        _mockFolderRepo = new Mock<IFolderRepository>();
        _mockOwnershipGuard = new Mock<OwnershipGuard>();
        
        _handler = new UpdateQuestionHandlerWithAuth(
            _mockQuestionRepo.Object,
            _mockFolderRepo.Object,
            _mockOwnershipGuard.Object
        );
    }

    [Test]
    public async Task Handle_NonOwnerCreator_ThrowsForbidden()
    {
        // Arrange
        var question = new Question(
            new QuestionText("Test?"),
            QuestionType.Text,
            Guid.NewGuid()
        )
        {
            CreatedByUserId = "other-user-123"
        };

        _mockQuestionRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(question);

        _mockOwnershipGuard
            .Setup(x => x.ValidateOwnership(It.IsAny<string>(), It.IsAny<string>()))
            .Throws<ForbiddenAccessException>();

        var userContext = new CurrentUserContext 
        { 
            UserId = "current-user-123",
            Role = "Creator"
        };
        
        var command = new UpdateQuestionCommandWithAuth(
            Guid.NewGuid(),
            "Updated text",
            userContext
        );

        // Act & Assert
        await Should.ThrowAsync<ForbiddenAccessException>(
            () => _handler.Handle(command)
        );
    }

    [Test]
    public async Task Handle_OwnerUpdates_Succeeds()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var question = new Question(
            new QuestionText("Original text"),
            QuestionType.Text,
            Guid.NewGuid()
        )
        {
            Id = questionId,
            CreatedByUserId = "alice-123"
        };

        _mockQuestionRepo
            .Setup(x => x.GetByIdAsync(questionId))
            .ReturnsAsync(question);

        _mockOwnershipGuard
            .Setup(x => x.ValidateOwnership(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        var userContext = new CurrentUserContext 
        { 
            UserId = "alice-123",
            Role = "Creator"
        };

        var command = new UpdateQuestionCommandWithAuth(
            questionId,
            "Updated text",
            userContext
        );

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.Text.Should().Contain("Updated");
        _mockOwnershipGuard.Verify();
    }

    [Test]
    public async Task Handle_AdminUpdates_Succeeds()
    {
        // Arrange
        var question = new Question(
            new QuestionText("Original"),
            QuestionType.Text,
            Guid.NewGuid()
        )
        {
            CreatedByUserId = "other-user-123"  // Not the same as current user
        };

        _mockQuestionRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(question);

        _mockOwnershipGuard
            .Setup(x => x.ValidateOwnership(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();  // Should be called and pass for admin

        var userContext = new CurrentUserContext 
        { 
            UserId = "admin-123",
            Role = "Admin"  // Admin role
        };

        var command = new UpdateQuestionCommandWithAuth(
            Guid.NewGuid(),
            "Updated",
            userContext
        );

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        _mockOwnershipGuard.Verify();
    }
}
*/


/*
╔══════════════════════════════════════════════════════════════════════════════╗
║ PATTERN 5: Exception Middleware                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

// ============================================================================
// EXAMPLE 5: Exception Handling Middleware
// ============================================================================

/*
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Map exception to HTTP response
        var (statusCode, message) = exception switch
        {
            // 401 Unauthorized: Authentication failed
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication failed"),
            
            // 403 Forbidden: Authorization failed (user lacks permission)
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, exception.Message),
            
            // 404 Not Found
            InvalidOperationException when exception.Message.Contains("not found") =>
                (StatusCodes.Status404NotFound, exception.Message),
            
            // 400 Bad Request: Invalid input
            ArgumentException or ArgumentNullException =>
                (StatusCodes.Status400BadRequest, "Invalid request data"),
            
            // 500 Internal Server Error: Unexpected
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
*/


/*
╔══════════════════════════════════════════════════════════════════════════════╗
║ PATTERN 6: Dependency Injection Setup                                        ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

// ============================================================================
// EXAMPLE 6: Register All Handlers in DI
// ============================================================================

/*
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all secured handlers
        services.AddScoped<CreateFolderHandler>();
        services.AddScoped<UpdateFolderHandlerWithAuth>();
        services.AddScoped<DeleteFolderHandlerWithAuth>();
        services.AddScoped<GetFolderHandler>();

        services.AddScoped<CreateQuestionHandler>();
        services.AddScoped<UpdateQuestionHandlerWithAuth>();
        services.AddScoped<DeleteQuestionHandlerWithAuth>();
        services.AddScoped<GetQuestionHandler>();

        services.AddScoped<CreateQuestionGroupHandler>();
        services.AddScoped<UpdateQuestionGroupHandlerWithAuth>();
        services.AddScoped<DeleteQuestionGroupHandlerWithAuth>();
        services.AddScoped<GetQuestionGroupHandler>();

        // Security
        services.AddScoped<OwnershipGuard>();

        return services;
    }
}
*/


/*
╔══════════════════════════════════════════════════════════════════════════════╗
║ PATTERN 7: Custom Authorization Policies                                     ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

// ============================================================================
// EXAMPLE 7: Complex Authorization Requirements
// ============================================================================

/*
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    // Admin only
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Creator or Admin
    options.AddPolicy("CreatorOrAdmin", policy =>
        policy.RequireRole("Creator", "Admin"));

    // Any authenticated user
    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());

    // Multiple requirements
    options.AddPolicy("CreatorWithPendingQuizzes", policy =>
        policy.RequireRole("Creator", "Admin")
              .Requirements.Add(new HasPendingQuizzesRequirement())
    );
});

// Custom requirement handler (if needed)
public class HasPendingQuizzesRequirement : IAuthorizationRequirement { }

public class HasPendingQuizzesHandler : AuthorizationHandler<HasPendingQuizzesRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasPendingQuizzesRequirement requirement)
    {
        // Custom logic: Check if user has pending quizzes
        if (context.User.HasClaim(c => c.Type == "pending_quizzes"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

// Usage in controller
[Authorize(Policy = "CreatorOrAdmin")]
public IActionResult CreateQuiz() { ... }
*/


/*
╔══════════════════════════════════════════════════════════════════════════════╗
║ PATTERN 8: Query Objects (Read-Only, No Authorization Checks)                ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

// ============================================================================
// EXAMPLE 8: Read Operations Don't Need Ownership Checks
// ============================================================================

/*
[HttpGet("{questionId:guid}")]
[Authorize(Roles = "Admin, Creator, Viewer")]  // Anyone can read
public async Task<ActionResult<QuestionDto>> GetQuestion(Guid questionId)
{
    try
    {
        var query = new GetQuestionQuery(questionId);
        var result = await _handler.Handle(query);
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

// Handler: No authorization needed for read
public class GetQuestionHandler
{
    public async Task<QuestionDto?> Handle(GetQuestionQuery query)
    {
        // Just fetch and return - no ownership check needed
        var question = await _questionRepository.GetByIdAsync(query.QuestionId);
        
        if (question?.IsDeleted == true)
            return null;

        return MapToDto(question);
    }
}
*/


// End of examples

namespace QuizSystem.Api.Examples
{
    // This file is documentation only
}
