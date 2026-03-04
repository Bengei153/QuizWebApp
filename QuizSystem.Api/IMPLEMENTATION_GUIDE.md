╔══════════════════════════════════════════════════════════════════════════════╗
║          IMPLEMENTATION GUIDE: JWT Auth + Authorization Integration          ║
║                    Quiz System API - Production Setup                         ║
╚══════════════════════════════════════════════════════════════════════════════╝

Generated: January 31, 2026

TABLE OF CONTENTS
1. What Was Implemented
2. Key Files & Changes
3. How to Complete the Refactoring
4. Database Migration Steps
5. Testing Instructions
6. Deployment Checklist


╔══════════════════════════════════════════════════════════════════════════════╗
║ 1. WHAT WAS IMPLEMENTED                                                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

✓ COMPLETED:

Authentication Infrastructure:
  ✓ Program.cs: Enhanced JWT configuration with:
    - Complete token validation parameters
    - Role claim extraction
    - Events for debugging
    - Authorization policies
  
  ✓ ICurrentUserService: Extended with role support
    - UserId property
    - UserRole property
    - UserRoles collection
    - HasRole(role) method
    - IsAdmin property

  ✓ CurrentUserService: Implementation with role extraction
    - Extracts user ID from JWT 'sub' claim
    - Extracts roles from JWT 'role' claim
    - Testable without HttpContext
    - Case-insensitive role comparison

Domain Model Updates:
  ✓ BaseEntity: Enhanced with audit fields
    - CreatedAt (UTC timestamp)
    - UpdatedAt (nullable UTC timestamp)
    - IsDeleted (soft delete flag)
    - DeletedAt (nullable deletion timestamp)

  ✓ Folder, Question, QuestionGroup: 
    - Removed duplicate soft delete fields
    - Added SoftDelete() and Restore() methods
    - Added Update() methods with timestamp
    - CreatedByUserId for ownership tracking

Security Infrastructure:
  ✓ ForbiddenAccessException: New domain exception
    - Returns 403 status code
    - Used for authorization failures

  ✓ OwnershipGuard: Centralized authorization helper
    - ValidateOwnership(resourceOwnerId, resourceName)
    - ValidateOwnershipOrCondition(resourceOwnerId, condition)
    - IsCurrentUserOwner(resourceOwnerId)
    - IsCurrentUserAdmin()
    - GetCurrentUserIdOrThrow()

  ✓ CurrentUserContext DTO:
    - UserId property
    - Role property
    - Convenience properties: IsAdmin, IsCreator, IsViewer
    - HasRole(role) method

Application Layer:
  ✓ Enhanced Handlers:
    - UpdateFolderHandlerWithAuth: Update with ownership validation
    - DeleteFolderHandlerWithAuth: Soft delete with ownership validation

  ✓ Enhanced Commands:
    - UpdateFolderCommandWithAuth: Includes CurrentUserContext
    - DeleteFolderCommandWithAuth: Includes CurrentUserContext

API Layer:
  ✓ SecuredFolderController: Example implementation
    - [Authorize] attributes
    - Role-based access control
    - User extraction from JWT claims
    - CurrentUserContext creation and passing
    - Comprehensive error handling (401, 403, 404, 400)
    - Detailed XML documentation

  ✓ SecuredQuestionController: Template for other resources
    - Demonstrates patterns for Questions
    - TODO comments for implementation


╔══════════════════════════════════════════════════════════════════════════════╗
║ 2. KEY FILES & CHANGES                                                       ║
╚══════════════════════════════════════════════════════════════════════════════╝

INFRASTRUCTURE LAYER:
  
  Domain/Common/BaseEntity.cs
    • Added: CreatedAt, UpdatedAt, IsDeleted, DeletedAt
    • Impact: All entities inherit these properties
    • Migration: Add columns to ALL tables
  
  Domain/Common/ForbiddenAccessException.cs (NEW)
    • Thrown when user unauthorized to access resource
    • Maps to 403 Forbidden
  
  Infrastructure/Repositories/CurrentUserService.cs
    • Added: UserRole, UserRoles, HasRole(), IsAdmin
    • Enhanced to extract role claims from JWT
  
  Application/Abstractions/Persistence/ICurrentUserService.cs
    • Updated: Added role-related properties and methods
    • Influence: All implementations must follow interface


APPLICATION LAYER:
  
  Application/Dtos/CurrentUserContext.cs (NEW)
    • DTO for passing user info through handlers
    • Passed from controller (extracted from JWT)
    • Never populated from request body
  
  Application/Dtos/FolderDto.cs
    • Added: CreatedByUserId, CreatedAt, UpdatedAt, IsDeleted
    • Impact: API responses now include audit info
  
  Application/Security/OwnershipGuard.cs (NEW)
    • Centralized authorization validation
    • Dependency for all update/delete handlers
  
  Application/Features/Folders/UpdateFolderCommandWithAuth.cs (NEW)
    • Record with CurrentUserContext parameter
  
  Application/Features/Folders/DeleteFolderCommandWithAuth.cs (NEW)
    • Record with CurrentUserContext parameter
  
  Application/Features/Folders/UpdateFolderHandlerWithAuth.cs (NEW)
    • Handler with authorization checks
    • Model for all update handlers
  
  Application/Features/Folders/DeleteFolderHandlerWithAuth.cs (NEW)
    • Handler with soft delete
    • Model for all delete handlers


DOMAIN LAYER:
  
  Domain/Entities/Folder.cs
    • Removed: Duplicate IsDeleted, DeletedAt (now in BaseEntity)
    • Added: SoftDelete(), Restore(), Update() methods
    • Added: CreatedAt initialization
  
  Domain/Entities/Question.cs
    • Removed: Duplicate IsDeleted, DeletedAt
    • Added: SoftDelete(), Restore(), UpdateText() methods
    • Added: CreatedAt initialization
  
  Domain/Entities/QuestionGroup.cs
    • Removed: Duplicate IsDeleted, DeletedAt
    • Added: SoftDelete(), Restore(), Update() methods
    • Added: CreatedAt initialization


PRESENTATION LAYER:
  
  Api/Controllers/SecuredFolderController.cs (NEW)
    • Complete example of secured endpoints
    • All CRUD operations with authorization
    • Error handling for 401, 403, 404, 400
    • Request/Response DTOs

  Api/Controllers/SecuredQuestionController.cs (NEW)
    • Template for Questions
    • TODO comments for implementation


CONFIGURATION:
  
  Program.cs
    • Enhanced JWT validation configuration
    • Added authorization policies
    • Added OwnershipGuard to DI


╔══════════════════════════════════════════════════════════════════════════════╗
║ 3. HOW TO COMPLETE THE REFACTORING                                           ║
╚══════════════════════════════════════════════════════════════════════════════╝

STEP 1: DATABASE MIGRATION
  
  In Package Manager Console:
    Add-Migration AddAuditFieldsAndSoftDelete
    Update-Database
  
  OR manually run SQL:
    ALTER TABLE QuestionGroups ADD 
      CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
      UpdatedAt DATETIME NULL,
      IsDeleted BIT NOT NULL DEFAULT 0,
      DeletedAt DATETIME NULL
    
    -- Repeat for Folders, Questions, Answers, QuestionOptions, QuestionOptions


STEP 2: UPDATE EXISTING HANDLERS

  Pattern for UpdateFolderHandler (OLD) → UpdateFolderHandlerWithAuth (NEW):
  
    OLD CODE:
      public class UpdateFolderHandler
      {
          public async Task<FolderDto> Handle(UpdateFolderCommand command)
          {
              var folder = await _folderRepository.GetByIdAsync(...);
              folder.Name = command.Name;
              await _folderRepository.UpdateAsync(folder);
              return new FolderDto { ... };
          }
      }
    
    NEW CODE:
      public class UpdateFolderHandler
      {
          private readonly OwnershipGuard _ownershipGuard;
          
          public async Task<FolderDto> Handle(UpdateFolderCommand command)
          {
              // 1. Authorization check
              _ownershipGuard.ValidateOwnership(folder.CreatedByUserId, "Folder");
              
              // 2. Business logic
              var folder = await _folderRepository.GetByIdAsync(...);
              folder.Update(command.Name);  // Now sets UpdatedAt
              
              // 3. Persistence
              await _folderRepository.UpdateAsync(folder);
              
              return MapToDto(folder);
          }
      }
  
  DO THIS FOR ALL:
    • Questions: Create, Update, Delete
    • Folders: Create, Update, Delete
    • QuestionGroups: Create, Update, Delete


STEP 3: UPDATE CONTROLLERS

  OLD CODE (FolderController):
    [HttpPut]
    public async Task<IActionResult> Update(UpdateFolderCommand command)
    {
        await _handler.Handle(command);
        return Ok();
    }
  
  NEW CODE (SecuredFolderController):
    [HttpPut]
    [Authorize(Roles = "Admin, Creator")]
    public async Task<IActionResult> Update(Guid folderId, UpdateFolderRequestDto request)
    {
        var userId = _currentUserService.UserId;
        var userContext = new CurrentUserContext 
        { 
            UserId = userId, 
            Role = _currentUserService.UserRole 
        };
        
        var command = new UpdateFolderCommandWithAuth(folderId, groupId, request.Name, userContext);
        var result = await _handler.Handle(command);
        
        return Ok(result);
    }
  
  DO THIS FOR ALL endpoints:
    • Add [Authorize] attribute with appropriate roles
    • Extract user from JWT claims
    • Create CurrentUserContext
    • Pass to handler
    • Handle ForbiddenAccessException → return Forbid()


STEP 4: UPDATE REPOSITORIES

  All repository queries must filter soft deleted items:
  
    OLD: return await _dbContext.Folders.Where(f => f.GroupId == groupId).ToListAsync();
    
    NEW: return await _dbContext.Folders
             .Where(f => f.GroupId == groupId && !f.IsDeleted)
             .ToListAsync();
  
  Add query extension method for reuse:
    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> query) 
        where T : BaseEntity
    {
        return query.Where(x => !x.IsDeleted);
    }
    
    Usage: return await _dbContext.Folders.NotDeleted().ToListAsync();


STEP 5: UPDATE EXCEPTION MIDDLEWARE

  Ensure ExceptionHandlingMiddleware catches ForbiddenAccessException:
  
    catch (ForbiddenAccessException ex)
    {
        response.StatusCode = StatusCodes.Status403Forbidden;
        return new { message = ex.Message };
    }


STEP 6: UPDATE DATA TRANSFER OBJECTS

  Add audit fields to all DTOs:
  
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string CreatedByUserId { get; set; }  // NEW
        public DateTime CreatedAt { get; set; }      // NEW
        public DateTime? UpdatedAt { get; set; }     // NEW
        public bool IsDeleted { get; set; }          // NEW
    }


╔══════════════════════════════════════════════════════════════════════════════╗
║ 4. DATABASE MIGRATION STEPS                                                  ║
╚══════════════════════════════════════════════════════════════════════════════╝

USING ENTITY FRAMEWORK CORE:

  1. Create migration:
     Add-Migration AddAuditFieldsToAllEntities

  2. Review migration file (Migrations/[timestamp]_AddAuditFieldsToAllEntities.cs):
     Should have Up() and Down() methods that add columns

  3. Apply migration:
     Update-Database

  4. Verify: Check SQL Server that columns were added


MANUAL SQL (if not using EF migrations):

  For each table, add columns:
  
    ALTER TABLE QuestionGroups ADD 
      CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
      UpdatedAt DATETIME2 NULL,
      IsDeleted BIT NOT NULL DEFAULT 0,
      DeletedAt DATETIME2 NULL;
    
    ALTER TABLE Folders ADD 
      CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
      UpdatedAt DATETIME2 NULL,
      IsDeleted BIT NOT NULL DEFAULT 0,
      DeletedAt DATETIME2 NULL;
    
    ALTER TABLE Questions ADD 
      CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
      UpdatedAt DATETIME2 NULL,
      IsDeleted BIT NOT NULL DEFAULT 0,
      DeletedAt DATETIME2 NULL;
    
    ALTER TABLE Answers ADD 
      CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
      UpdatedAt DATETIME2 NULL,
      IsDeleted BIT NOT NULL DEFAULT 0,
      DeletedAt DATETIME2 NULL;
    
    ALTER TABLE QuestionOptions ADD 
      CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
      UpdatedAt DATETIME2 NULL,
      IsDeleted BIT NOT NULL DEFAULT 0,
      DeletedAt DATETIME2 NULL;

  Add NOT NULL constraint to CreatedByUserId if not already:
  
    ALTER TABLE QuestionGroups 
      ALTER COLUMN CreatedByUserId NVARCHAR(256) NOT NULL;
    
    -- Repeat for other tables


CREATE INDEXES for soft delete queries (important for performance!):

  CREATE INDEX IX_QuestionGroups_IsDeleted 
    ON QuestionGroups(IsDeleted)
    WHERE IsDeleted = 0;
  
  -- Repeat for Folders, Questions, etc.


╔══════════════════════════════════════════════════════════════════════════════╗
║ 5. TESTING INSTRUCTIONS                                                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

MANUAL TESTING WITH POSTMAN:

  1. Get JWT Token from Auth API
     POST http://auth-api:5000/api/auth/login
     Body: { "username": "testuser", "password": "password" }
     Response: { "token": "eyJhbGc..." }

  2. Create Folder (should work)
     POST http://localhost:5170/api/question-groups/123/folders
     Headers: Authorization: Bearer eyJhbGc...
     Body: { "name": "My Folder" }
     Expected: 201 Created

  3. Try without Authorization header
     Expected: 401 Unauthorized

  4. Try with invalid token
     Expected: 401 Unauthorized

  5. Try with expired token
     Expected: 401 Unauthorized

  6. Update Folder as non-owner
     Update folder created by other user
     Expected: 403 Forbidden

  7. Update Folder as owner
     Update own folder
     Expected: 200 OK

  8. Update Folder as Admin
     Admin updates any folder
     Expected: 200 OK

  9. Delete Folder
     DELETE http://localhost:5170/api/...
     Expected: 204 No Content
     Then GET: Item should not appear (soft deleted)

 10. Check database
     SELECT * FROM Folders WHERE Id = '...'
     Should see: IsDeleted = 1, DeletedAt = timestamp


UNIT TESTS EXAMPLE:

  [TestFixture]
  public class UpdateFolderHandlerWithAuthTests
  {
      private Mock<IFolderRepository> _mockRepo;
      private Mock<OwnershipGuard> _mockGuard;
      private UpdateFolderHandlerWithAuth _handler;

      [SetUp]
      public void Setup()
      {
          _mockRepo = new Mock<IFolderRepository>();
          _mockGuard = new Mock<OwnershipGuard>();
          _handler = new UpdateFolderHandlerWithAuth(_mockRepo.Object, _mockGuard.Object);
      }

      [Test]
      public async Task Handle_UserNotOwner_ThrowsForbidden()
      {
          var folder = new Folder("Old Name", Guid.NewGuid()) 
          { 
              CreatedByUserId = "bob-123" 
          };
          
          _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
              .ReturnsAsync(folder);
          
          _mockGuard.Setup(x => x.ValidateOwnership(It.IsAny<string>(), It.IsAny<string>()))
              .Throws<ForbiddenAccessException>();

          var userContext = new CurrentUserContext { UserId = "alice-123" };
          var command = new UpdateFolderCommandWithAuth(Guid.NewGuid(), Guid.NewGuid(), "New Name", userContext);

          await Should.ThrowAsync<ForbiddenAccessException>(() => _handler.Handle(command));
      }

      [Test]
      public async Task Handle_OwnerUpdates_Succeeds()
      {
          var folderId = Guid.NewGuid();
          var groupId = Guid.NewGuid();
          var folder = new Folder("Old Name", groupId) 
          { 
              Id = folderId,
              CreatedByUserId = "alice-123" 
          };
          
          _mockRepo.Setup(x => x.GetByIdAsync(groupId, folderId))
              .ReturnsAsync(folder);
          
          _mockGuard.Setup(x => x.ValidateOwnership(It.IsAny<string>(), It.IsAny<string>()))
              .Verifiable();

          var userContext = new CurrentUserContext { UserId = "alice-123" };
          var command = new UpdateFolderCommandWithAuth(folderId, groupId, "New Name", userContext);

          var result = await _handler.Handle(command);

          result.Should().NotBeNull();
          result.Name.Should().Be("New Name");
          _mockGuard.Verify();
      }
  }


╔══════════════════════════════════════════════════════════════════════════════╗
║ 6. DEPLOYMENT CHECKLIST                                                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

PRE-DEPLOYMENT (Development Environment):

  ☐ Compile solution without errors: dotnet build
  
  ☐ Run all unit tests: dotnet test
  
  ☐ Run integration tests with JWT tokens
  
  ☐ Test all CRUD operations:
    - Create with correct role
    - Create with wrong role (should fail)
    - Update as owner (should succeed)
    - Update as non-owner (should fail: 403)
    - Delete as owner (soft delete)
    - Delete non-existent item (404)
  
  ☐ Test error scenarios:
    - No Authorization header: 401
    - Invalid token: 401
    - Expired token: 401
    - Unauthorized role: 403
    - Not owner: 403
    - Invalid input: 400


STAGING/PRODUCTION DEPLOYMENT:

  ☐ Database migration applied:
    - Backup database first
    - Run migration: Update-Database -Environment Production
    - Verify columns added
    - No data loss
  
  ☐ Secrets configured:
    - JWT key in Key Vault (not in code)
    - Connection string in Key Vault
    - Issuer and Audience correct for production Auth API
  
  ☐ appsettings.Production.json:
    {
      "Jwt": {
        "validIssuer": "ProductionAuthAPI",
        "validAudience": "https://api.example.com",
        "key": "${JWT_SECRET_KEY}"  // From Key Vault
      }
    }
  
  ☐ HTTPS enforced: All traffic is HTTPS (not HTTP)
  
  ☐ CORS configured properly (not * wildcard)
  
  ☐ Rate limiting enabled
  
  ☐ Monitoring and logging enabled
  
  ☐ Swagger/OpenAPI documentation updated
  
  ☐ API versioning strategy confirmed
  
  ☐ Load testing performed to verify authorization doesn't cause bottlenecks


ROLLBACK PLAN:

  If issues occur after deployment:
  
  ☐ Database rollback:
    Update-Database -TargetMigration PreviousMigration -Environment Production
  
  ☐ Code rollback:
    Redeploy previous version
  
  ☐ Notify affected users
  
  ☐ Document what went wrong
  
  ☐ Run post-mortem


═══════════════════════════════════════════════════════════════════════════════

NEXT STEPS:

1. Run: dotnet build
   → Fix any compilation errors

2. Apply database migration:
   → Add-Migration AddAuditFieldsAndSoftDelete
   → Update-Database

3. Refactor remaining handlers:
   → Follow patterns in UpdateFolderHandlerWithAuth
   → Apply to all Create/Update/Delete operations

4. Update all controllers:
   → Follow pattern in SecuredFolderController
   → Apply to all endpoints

5. Write and run tests

6. Deploy to staging

7. Deploy to production


═══════════════════════════════════════════════════════════════════════════════

QUESTIONS OR ISSUES?

Refer to AUTHORIZATION_GUIDE.md for comprehensive documentation
on authentication, authorization, and security patterns.

═══════════════════════════════════════════════════════════════════════════════
