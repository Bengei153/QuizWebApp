/*
╔══════════════════════════════════════════════════════════════════════════════╗
║                      AUTHENTICATION & AUTHORIZATION GUIDE                    ║
║                      Production-Grade Security Implementation                ║
╚══════════════════════════════════════════════════════════════════════════════╝

TABLE OF CONTENTS
1. Architecture Overview
2. Authentication Flow
3. Authorization Patterns
4. Domain Model Security
5. Ownership Rules
6. Role-Based Access Control (RBAC)
7. Error Handling
8. Testing Considerations
9. Production Checklist


╔══════════════════════════════════════════════════════════════════════════════╗
║ 1. ARCHITECTURE OVERVIEW                                                     ║
╚══════════════════════════════════════════════════════════════════════════════╝

SEPARATION OF CONCERNS:
    API Layer (Controllers)
    ↓ Extracts user from JWT claims
    ↓ Creates CurrentUserContext DTO
    ↓ Passes to handlers
    ├─ Application Layer (Handlers)
    │  ↓ Contains business logic
    │  ↓ Uses OwnershipGuard for authorization
    │  ↓ Throws ForbiddenAccessException if unauthorized
    │  ├─ Domain Layer (Entities)
    │     ↓ Contains CreatedByUserId
    │     ↓ Contains audit fields (CreatedAt, UpdatedAt)
    │     ↓ Supports soft delete


SECURITY PRINCIPLES:
✓ User ID is EXTRACTED from JWT claims in the API layer
✗ User ID is NEVER accepted from request body
✓ Authorization logic is centralized in OwnershipGuard
✗ Authorization is NOT duplicated across handlers
✓ All handlers are testable (no HttpContext dependency)
✗ Handlers don't access HttpContext directly
✓ Soft deletes preserve all data
✗ Hard deletes are never used


╔══════════════════════════════════════════════════════════════════════════════╗
║ 2. AUTHENTICATION FLOW                                                       ║
╚══════════════════════════════════════════════════════════════════════════════╝

REQUEST:
    POST /api/folders/123/update
    Authorization: Bearer eyJhbGc... (JWT Token from Auth API)

FLOW:
    1. JwtBearer middleware validates token:
       - Checks signature matches Auth API's signing key
       - Verifies issuer: "RealWorldAPI"
       - Verifies audience: "http://localhost:5170"
       - Validates expiry: token hasn't expired
       - Verifies signing key is valid

    2. Token is decoded and claims are extracted:
       - sub (subject): User ID
       - role: User role (Admin, Creator, Viewer)
       - exp (expiry): Token expiration time
       - Other claims as defined by Auth API

    3. ClaimsPrincipal is created and attached to HttpContext
       - Available via User property in controllers
       - Can be accessed via ICurrentUserService

    4. [Authorize] attribute checks:
       - Is user authenticated? (Has valid ClaimsPrincipal)
       - If using [Authorize(Roles = "...")]:
         Are they in the required roles?

    5. If authenticated, request proceeds
       If not, 401 Unauthorized is returned


JWT TOKEN STRUCTURE:
    Header:     { "alg": "HS256", "typ": "JWT" }
    Payload:    { "sub": "user-id-123", "role": "Creator", "exp": 1735689600, ... }
    Signature:  HMACSHA256(base64(header) + "." + base64(payload), secret)

CRITICAL: Header and Payload are Base64-encoded, NOT encrypted.
The signature proves the token hasn't been tampered with.


╔══════════════════════════════════════════════════════════════════════════════╗
║ 3. AUTHORIZATION PATTERNS                                                    ║
╚══════════════════════════════════════════════════════════════════════════════╝

PATTERN 1: Role-Based Access Control
    [Authorize(Roles = "Admin, Creator")]
    public IActionResult Create() { ... }
    
    ✓ Use: For endpoint-level checks
    ✓ Example: Only Creators and Admins can create folders
    ✓ Note: This is authentication, verified by JWT middleware


PATTERN 2: Ownership Validation (Critical!)
    In handler:
        _ownershipGuard.ValidateOwnership(folder.CreatedByUserId, "Folder");
    
    ✓ Use: For resource-level authorization
    ✓ Example: User can only update/delete their own folder
    ✓ Admin can update/delete any folder
    ✓ If unauthorized: Throws ForbiddenAccessException (403)


PATTERN 3: Extract User from Claims
    In controller:
        var userId = _currentUserService.UserId;           // From JWT 'sub'
        var userRole = _currentUserService.UserRole;       // From JWT 'role'
    
    ✓ NEVER from request body: int userId = request.UserId;  ✗✗✗
    ✓ Pass to handler via CurrentUserContext
    ✓ This prevents authorization bypass


PATTERN 4: Pass User Context to Handlers
    In controller:
        var userContext = new CurrentUserContext {
            UserId = userId,
            Role = userRole
        };
        var result = await handler.Handle(new UpdateCommand(..., userContext));
    
    ✓ Handlers are testable (no HttpContext needed)
    ✓ User context flows through CQRS pipeline
    ✓ Easy to mock in unit tests


╔══════════════════════════════════════════════════════════════════════════════╗
║ 4. DOMAIN MODEL SECURITY                                                     ║
╚══════════════════════════════════════════════════════════════════════════════╝

AUDIT TRAIL FIELDS (in BaseEntity):
    CreatedAt:      DateTime        When resource was created
    UpdatedAt:      DateTime?       When resource was last updated
    IsDeleted:      bool            Soft delete flag
    DeletedAt:      DateTime?       When resource was soft deleted

PER-RESOURCE SECURITY (in each entity):
    CreatedByUserId:    string      User who created this resource
    
    Used for ownership validation:
    - Can user update/delete? If CreatedByUserId == UserId OR IsAdmin


EXAMPLE: Folder Entity
    public class Folder : BaseEntity  // Inherits CreatedAt, UpdatedAt, IsDeleted, DeletedAt
    {
        public string Name { get; set; }
        public string CreatedByUserId { get; set; }  // Who created this folder
        
        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }

DATABASE DESIGN:
    Folders
    ├─ Id              GUID          Primary key
    ├─ Name            VARCHAR(255)  Folder name
    ├─ CreatedByUserId VARCHAR(256)  User who created
    ├─ CreatedAt       DATETIME      Created timestamp
    ├─ UpdatedAt       DATETIME?     Last update timestamp
    ├─ IsDeleted       BIT           Soft delete flag
    ├─ DeletedAt       DATETIME?     Deletion timestamp
    └─ QuestionGroupId GUID          Foreign key

QUERIES MUST FILTER DELETED ITEMS:
    SELECT * FROM Folders WHERE QuestionGroupId = @groupId AND IsDeleted = 0
    ✗ SELECT * FROM Folders WHERE QuestionGroupId = @groupId  (WRONG!)


╔══════════════════════════════════════════════════════════════════════════════╗
║ 5. OWNERSHIP RULES (CRITICAL)                                                ║
╚══════════════════════════════════════════════════════════════════════════════╝

OWNERSHIP VALIDATION ALGORITHM:
    function ValidateOwnership(resourceOwnerId: string, userId: string, isAdmin: bool)
    {
        if string.IsNullOrWhiteSpace(userId)
            throw ForbiddenAccessException("Not authenticated")
        
        if userId.EqualsIgnoreCase(resourceOwnerId)
            return  // User owns the resource - AUTHORIZED
        
        if isAdmin
            return  // User is admin - AUTHORIZED
        
        throw ForbiddenAccessException("You don't own this resource and aren't admin")
    }


SCENARIOS:

Scenario 1: Creator Updates Own Resource
    User ID:    alice-123
    Resource Owner: alice-123
    Is Admin:   false
    Result:     ✓ ALLOWED

Scenario 2: Creator Tries to Update Someone Else's Resource
    User ID:    alice-123
    Resource Owner: bob-456
    Is Admin:   false
    Result:     ✗ FORBIDDEN (403)

Scenario 3: Admin Updates Someone Else's Resource
    User ID:    admin-789
    Resource Owner: alice-123
    Is Admin:   true
    Result:     ✓ ALLOWED (admins bypass ownership check)

Scenario 4: Viewer Tries to Update
    User ID:    viewer-321
    Is Admin:   false
    Result:     ✗ FORBIDDEN (403) - viewers are read-only
    Note: [Authorize(Roles = "Creator")] stops them first


IMPLEMENTATION:
    // In UpdateFolderHandlerWithAuth
    public async Task<FolderDto> Handle(UpdateFolderCommandWithAuth command)
    {
        var folder = await _folderRepository.GetByIdAsync(command.GroupId, command.FolderId);
        
        // THE CRITICAL CHECK
        _ownershipGuard.ValidateOwnership(folder.CreatedByUserId, "Folder");
        
        folder.Update(command.Name);
        await _folderRepository.UpdateAsync(folder);
        
        return MapToDto(folder);
    }


╔══════════════════════════════════════════════════════════════════════════════╗
║ 6. ROLE-BASED ACCESS CONTROL (RBAC)                                          ║
╚══════════════════════════════════════════════════════════════════════════════╝

THREE ROLES DEFINED:

┌─ ADMIN ─────────────────────────────────────────────────────────────────────┐
│ Permissions:                                                                │
│  ✓ Create, Read, Update, Delete any resource                               │
│  ✓ Bypass ownership checks                                                  │
│  ✓ Access admin-only endpoints                                              │
│  ✓ View audit logs and soft-deleted items                                   │
│                                                                              │
│ Use cases:                                                                   │
│  - Support staff who need to manage user content                             │
│  - System administrators                                                     │
│  - Compliance officers                                                       │
│                                                                              │
│ Controllers: [Authorize(Roles = "Admin")] or [Authorize] (admins included) │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ CREATOR ───────────────────────────────────────────────────────────────────┐
│ Permissions:                                                                │
│  ✓ Create, Read, Update, Delete OWN resources                               │
│  ✗ Cannot modify other users' resources                                     │
│  ✓ Read resources from other creators (if public)                           │
│                                                                              │
│ Use cases:                                                                   │
│  - Quiz authors who create quiz content                                      │
│  - Teachers building quizzes                                                 │
│  - Content creators                                                          │
│                                                                              │
│ Controllers: [Authorize(Roles = "Admin, Creator")]                          │
│ Handlers: OwnershipGuard.ValidateOwnership() checks ownership               │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ VIEWER ────────────────────────────────────────────────────────────────────┐
│ Permissions:                                                                │
│  ✓ Read (GET) public resources                                              │
│  ✗ Cannot create, update, or delete anything                                │
│  ✗ Cannot access other users' private resources                             │
│                                                                              │
│ Use cases:                                                                   │
│  - Students taking quizzes                                                   │
│  - Read-only access to public content                                        │
│                                                                              │
│ Controllers: [Authorize(Roles = "Admin, Creator, Viewer")] for GET only    │
│             [Authorize(Roles = "Admin, Creator")] for POST/PUT/DELETE      │
└─────────────────────────────────────────────────────────────────────────────┘


EXAMPLE API AUTHORIZATION:

CreateFolder:   [Authorize(Roles = "Admin, Creator")]
GetFolder:      [Authorize(Roles = "Admin, Creator, Viewer")]
UpdateFolder:   [Authorize(Roles = "Admin, Creator")] + OwnershipGuard
DeleteFolder:   [Authorize(Roles = "Admin, Creator")] + OwnershipGuard
ListFolders:    [Authorize(Roles = "Admin, Creator, Viewer")]


╔══════════════════════════════════════════════════════════════════════════════╗
║ 7. ERROR HANDLING & RESPONSES                                                ║
╚══════════════════════════════════════════════════════════════════════════════╝

HTTP STATUS CODES:

200 OK
    Description: Request succeeded
    Example: GET /folders/123 returns folder details

201 Created
    Description: Resource was successfully created
    Example: POST /folders returns created folder with Location header

204 No Content
    Description: Request succeeded but no content to return
    Example: DELETE /folders/123 soft deletes the folder

400 Bad Request
    Description: Invalid request data
    Example: POST /folders with empty name

401 Unauthorized
    Description: Authentication failed or missing
    Causes:
        - No Authorization header provided
        - Invalid/expired JWT token
        - Token signature verification failed
        - Token issuer/audience mismatch
    Response:
        {
            "message": "Missing or invalid authentication token",
            "statusCode": 401
        }

403 Forbidden
    Description: Authentication succeeded but user lacks permission
    Causes:
        - User doesn't own the resource and isn't admin
        - User role doesn't allow the operation
        - [Authorize(Roles = "Creator")] but user is Viewer
    Response (from OwnershipGuard):
        {
            "message": "You do not have permission to modify this resource",
            "statusCode": 403
        }

404 Not Found
    Description: Resource doesn't exist
    Example: GET /folders/nonexistent-id

500 Internal Server Error
    Description: Unexpected server error
    Example: Database connection failure
    Note: Details should NOT be exposed to client in production


EXCEPTION MAPPING:

ForbiddenAccessException
    ↓
    HTTP 403 Forbidden
    ↓
    return Forbid();  or  return StatusCode(403, new { message = ex.Message });

InvalidOperationException
    ↓
    HTTP 400 Bad Request (validation issues)
    HTTP 404 Not Found (resource not found)

ArgumentNullException / ValidationException
    ↓
    HTTP 400 Bad Request


╔══════════════════════════════════════════════════════════════════════════════╗
║ 8. TESTING CONSIDERATIONS                                                    ║
╚══════════════════════════════════════════════════════════════════════════════╝

UNIT TESTING HANDLERS (No HttpContext needed!):

Mock OwnershipGuard behavior:
    var mockOwnershipGuard = new Mock<OwnershipGuard>();
    mockOwnershipGuard
        .Setup(x => x.ValidateOwnership(It.IsAny<string>(), It.IsAny<string>()))
        .Throws<ForbiddenAccessException>();  // Simulate unauthorized

Test Ownership Validation:
    [Test]
    public async Task UpdateFolder_UserNotOwner_ThrowsForbidden()
    {
        var handler = new UpdateFolderHandlerWithAuth(mockRepo, mockGuard);
        var command = new UpdateFolderCommandWithAuth(folderId, groupId, "New Name",
            new CurrentUserContext { UserId = "alice-123", Role = "Creator" }
        );
        
        await Should.ThrowAsync<ForbiddenAccessException>(
            () => handler.Handle(command)
        );
    }

Test Successful Update:
    [Test]
    public async Task UpdateFolder_Owner_Succeeds()
    {
        var handler = new UpdateFolderHandlerWithAuth(mockRepo, mockGuard);
        var command = new UpdateFolderCommandWithAuth(folderId, groupId, "New Name",
            new CurrentUserContext { UserId = "alice-123", Role = "Creator" }
        );
        
        var result = await handler.Handle(command);
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
    }

INTEGRATION TESTING:

Test JWT Validation:
    GET /api/folders
    - Without Authorization header: 401 Unauthorized
    - With invalid token: 401 Unauthorized
    - With expired token: 401 Unauthorized
    - With valid token: 200 OK

Test Role-Based Access:
    POST /api/folders
    - With Viewer role: 403 Forbidden
    - With Creator role: 201 Created
    - With Admin role: 201 Created

Test Ownership Enforcement:
    PUT /api/folders/123
    - As owner: 200 OK
    - As non-owner creator: 403 Forbidden
    - As admin: 200 OK

Test Soft Delete:
    DELETE /api/folders/123
    - Returns 204 No Content
    - GET /api/folders returns empty (folder is soft deleted)
    - In database: IsDeleted = 1, DeletedAt = now


╔══════════════════════════════════════════════════════════════════════════════╗
║ 9. PRODUCTION CHECKLIST                                                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

SECURITY:
☐ JWT signing key is stored securely (Azure Key Vault, not appsettings.json)
☐ HTTPS is enforced in production (not HTTP)
☐ CORS is properly configured (not * wildcard)
☐ CSRF tokens are implemented if needed
☐ Rate limiting is implemented to prevent brute force
☐ API versioning is in place for backward compatibility

AUTHENTICATION:
☐ All JWT validation parameters are correct (issuer, audience, key)
☐ ClockSkew is set to TimeSpan.Zero (no tolerance for expiry)
☐ Token expiry times are reasonable (15-60 minutes)
☐ Refresh tokens are implemented for long-lived sessions

AUTHORIZATION:
☐ All endpoints have [Authorize] attributes
☐ Role-based access is correct for each endpoint
☐ OwnershipGuard is used for all update/delete operations
☐ ForbiddenAccessException returns 403 status code

DATA & AUDIT:
☐ All entities have CreatedByUserId, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
☐ Soft deletes are used everywhere (no hard deletes)
☐ Database queries filter: WHERE IsDeleted = 0
☐ Audit logs track who did what when

ERROR HANDLING:
☐ Sensitive error details are NOT exposed to clients
☐ Exception middleware handles all exceptions
☐ Proper HTTP status codes are returned
☐ Error responses are consistent format

TESTING:
☐ Unit tests for handlers (mocking ICurrentUserService)
☐ Integration tests for authentication/authorization
☐ Tests for ownership enforcement
☐ Tests for each role's capabilities

DEPLOYMENT:
☐ Secrets are injected via environment variables (not in code)
☐ API documentation (Swagger) is updated
☐ Rate limiting is configured
☐ Monitoring/logging is enabled


EXAMPLE: JWT Configuration in appsettings.Production.json
{
  "Jwt": {
    "validIssuer": "RealWorldAPI",
    "validAudience": "http://localhost:5170",  // Your API's audience
    "key": "$(JWT_SECRET_KEY)"  // Injected from environment variable
  }
}

DEPLOYMENT COMMAND:
docker run -e JWT_SECRET_KEY="your-super-secret-key" myapp:latest


*/

namespace QuizSystem.Api.Security.Documentation
{
    // This file is documentation only and not executed
}
