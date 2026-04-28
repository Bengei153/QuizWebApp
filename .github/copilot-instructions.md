# Copilot Instructions for QuizWebApp

## Quick Start

This is a quiz management system built with **ASP.NET Core 9.0** and **Entity Framework Core**, using PostgreSQL. The API follows **CQRS** (Command Query Responsibility Segregation) pattern with **direct handler invocation** (no MediatR) and uses **JWT authentication**.

### Build & Run

```powershell
cd QuizSystem.Api
dotnet restore
dotnet build
dotnet run
```

The API runs on `https://localhost:5170` with Swagger UI available at `/swagger`.

### Run Tests
```powershell
dotnet test
```

Currently, no test project exists in the repository. When creating tests, add them to a `QuizSystem.Tests` folder following the same namespace structure.

---

## Architecture Overview

### Layer Structure
The project follows **Clean Architecture** with four layers:

1. **Domain Layer** (`QuestionSystem.Domain`)
   - Pure entities and business rules
   - No external dependencies
   - Contains: `Folder`, `Question`, `QuestionGroup`, `QuizAttempt`, `Answer`, `QuestionOption`

2. **Application Layer** (`QuestionSystem.Application`)
   - Business logic using **CQRS** pattern
   - Handlers = business operations (Create, Update, Delete, Get)
   - Commands = write operations, Queries = read operations
   - Service abstractions (interfaces in `Abstractions/Persistence`)
   - DTOs for API contracts

3. **Infrastructure Layer** (`QuestionSystem.Infrastructure`)
   - Database persistence (AppDbContext)
   - Repository implementations
   - EF Core configurations
   - CurrentUserService (extracts User ID from JWT)

4. **API Layer** (`QuestionSystem.Api`)
   - Controllers that directly invoke handlers (no MediatR)
   - HTTP routing and exception handling
   - Middleware (authentication, error handling)

### Key Files

| File | Purpose |
|------|---------|
| `Program.cs` | DI setup, JWT configuration, authorization policies |
| `AppDbContext.cs` | EF Core context with all entity mappings |
| `ICurrentUserService.cs` | Extracts authenticated user ID from JWT claims |
| `OwnershipGuard.cs` | Authorization logic for resource ownership |
| `appsettings.json` | Database and JWT configuration |

---

## Key Conventions

### 1. Handler Pattern (Direct Invocation - No MediatR)
All business operations use handler classes with a `Handle()` method. Handlers are directly injected into controllers and called synchronously.

**Handler Structure:**
```csharp
public class CreateFolderHandler
{
    private readonly IFolderRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    
    public CreateFolderHandler(IFolderRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }
    
    public async Task Handle(CreateFolderCommand command)
    {
        var userId = _currentUserService.UserId; // Extract from JWT
        var folder = new Folder { Name = command.Name, CreatedByUserId = userId };
        await _repository.AddAsync(folder);
    }
}
```

**Controller Usage:**
```csharp
public class FolderController : ControllerBase
{
    private readonly CreateFolderHandler _handler;
    
    public FolderController(CreateFolderHandler handler)
    {
        _handler = handler;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFolderCommand command)
    {
        try
        {
            await _handler.Handle(command);
            return Ok(new { message = "Folder created successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
}
```

**Location**: `QuestionSystem.Application/Features/{Entity}/{Entity}{Handler}.cs` and `{Entity}{Command/Query}.cs`

**Key Points:**
- No MediatR dispatch - direct `await handler.Handle(command)`
- Handlers are scoped dependencies registered in DI container
- Commands/Queries are plain C# classes that hold input data
- Return types can be `void`, `Task`, or `Task<T>`

### 2. Ownership Authorization
Resources (Folders, QuestionGroups) have ownership checks:

```csharp
// In update/delete handlers:
var userId = _currentUserService.UserId;
var isAdmin = _currentUserService.IsAdmin; // Check role from JWT

if (folder.CreatedByUserId != userId && !isAdmin)
    throw new ForbiddenAccessException("Not authorized to modify this resource");

// Proceed with operation...
await _repository.UpdateAsync(folder);
```

**Key points**:
- User ID comes from JWT `sub` claim (cannot be spoofed)
- Ownership checked before modification
- Admins bypass ownership checks
- Use soft deletes (set `IsDeleted = true`) instead of hard deletes
- Controllers catch `ForbiddenAccessException` and return `Forbid()`

### 3. Entity Audit Fields
All entities have:
```csharp
public string CreatedByUserId { get; set; }
public DateTime CreatedAt { get; set; }
public string? UpdatedByUserId { get; set; }
public DateTime? UpdatedAt { get; set; }
public string? DeletedByUserId { get; set; }
public DateTime? DeletedAt { get; set; }
public bool IsDeleted { get; set; }
```

**When querying**: Always filter soft-deleted items:
```csharp
.Where(f => f.GroupId == groupId && !f.IsDeleted)
```

### 4. Repository Pattern
All data access goes through repositories. When adding new queries:

```csharp
public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id);
    Task<IEnumerable<Folder>> GetAllAsync();
    Task AddAsync(Folder folder);
    Task UpdateAsync(Folder folder);
}
```

**Never query DbContext directly** from handlers. Use repositories.

### 5. Exception Handling in Controllers
Controllers catch exceptions from handlers and return appropriate HTTP statuses:

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateFolder(Guid id, UpdateFolderCommand command)
{
    try
    {
        var result = await _updateHandler.Handle(command);
        return Ok(result);
    }
    catch (ForbiddenAccessException ex)
    {
        return Forbid(ex.Message); // 403
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return StatusCode(500, new { message = "An unexpected error occurred." });
    }
}
```

### 6. JWT Claims & Authorization
- User ID is in `sub` claim (configured in `Program.cs`: `NameClaimType = "sub"`)
- Roles in standard role claims (Admin, Creator, Viewer)
- Applied via `[Authorize(Roles = "...")]` attributes:

```csharp
[Authorize(Roles = "Creator")]
[HttpPost]
public async Task<IActionResult> Create(CreateFolderCommand command)
{
    await _handler.Handle(command);
    return Ok(new { message = "Created successfully" });
}
```

### 7. DTOs and Validation
- DTOs in `QuestionSystem.Application/Dtos/{Entity}Dto.cs`
- Validators in `QuestionSystem.Application/Features/{Entity}/Validations/{Entity}Validator.cs`
- Use FluentValidation for rules:

```csharp
public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);
    }
}
```

---

## Database

### Connection
- Uses **PostgreSQL** (see `appsettings.json` for connection string)
- Configured via EF Core in `Infrastructure/DependencyInjection.cs`
- DbContext: `AppDbContext`

### Migrations
Create new migrations when changing entities:
```powershell
cd QuizSystem.Api
dotnet ef migrations add MigrationName
dotnet ef database update
```

**Always** include the migration file in git so other developers can apply it.

### Key Entities
- `Folder` - Quiz containers with ownership
- `QuestionGroup` - Groups of questions within a folder
- `Question` - Individual quiz questions with options
- `QuizAttempt` - User's quiz attempt record
- `Answer` - User's answer to a question
- `QuestionOption` - Possible answers for a question

---

## Common Tasks

### Adding a New API Endpoint

1. **Create Command/Query** in `QuestionSystem.Application/Features/{Entity}/`
   ```csharp
   public class CreateXyzCommand
   {
       public string Name { get; set; }
   }
   ```

2. **Create Handler** in same folder
   ```csharp
   public class CreateXyzHandler
   {
       private readonly IXyzRepository _repository;
       private readonly ICurrentUserService _currentUserService;
       
       public CreateXyzHandler(IXyzRepository repository, ICurrentUserService currentUserService)
       {
           _repository = repository;
           _currentUserService = currentUserService;
       }
       
       public async Task Handle(CreateXyzCommand command)
       {
           var userId = _currentUserService.UserId;
           var xyz = new Xyz { Name = command.Name, CreatedByUserId = userId };
           await _repository.AddAsync(xyz);
       }
   }
   ```

3. **Create/Update DTO** in `QuestionSystem.Application/Dtos/`

4. **Add to Controller** in `QuestionSystem.Api/Controllers/{Entity}Controller.cs`
   ```csharp
   private readonly CreateXyzHandler _handler;
   
   public XyzController(CreateXyzHandler handler)
   {
       _handler = handler;
   }
   
   [HttpPost]
   public async Task<IActionResult> Create([FromBody] CreateXyzCommand command)
   {
       try
       {
           await _handler.Handle(command);
           return Ok(new { message = "Created successfully" });
       }
       catch (Exception ex)
       {
           return StatusCode(500, new { message = "An error occurred." });
       }
   }
   ```

5. **Register Handler** in `QuestionSystem.Application/DependencyInjecton.cs`
   ```csharp
   services.AddScoped<CreateXyzHandler>();
   ```

6. **Add migration** if entity changes: `dotnet ef migrations add {Name}`

### Adding Authorization to Existing Endpoint

1. **Update Handler** to check ownership:
   ```csharp
   var userId = _currentUserService.UserId;
   var isAdmin = _currentUserService.IsAdmin; // if available
   if (entity.CreatedByUserId != userId && !isAdmin)
       throw new ForbiddenAccessException();
   ```

2. **Update Controller** to catch exception:
   ```csharp
   catch (ForbiddenAccessException ex)
   {
       return Forbid(ex.Message); // Returns 403
   }
   ```

3. **Add [Authorize] attribute** if endpoint requires authentication
   ```csharp
   [Authorize(Roles = "Creator")]
   [HttpPost]
   public async Task<IActionResult> Create(CreateXyzCommand command) { }
   ```

### Querying with Soft Deletes
Always filter out soft-deleted items in repository queries:
```csharp
.Where(x => !x.IsDeleted)
```

---

## Important Notes

### Security
- **Never** extract User ID from request body - use `ICurrentUserService.UserId`
- **Always** validate ownership before Update/Delete operations
- **Store secrets** in environment variables or secrets manager, never in appsettings.json
- JWT validation is strict: issuer, audience, expiration all checked

### Performance
- Use `.AsNoTracking()` for read-only queries
- Implement pagination for large result sets (not currently done - consider adding)
- Add database indexes for frequently queried fields

### Data Integrity
- Use soft deletes instead of hard deletes
- Include audit fields (CreatedAt, UpdatedAt, DeletedAt, CreatedByUserId)
- Migrations must be idempotent

---

## Debugging Tips

### Check JWT Claims
In `Program.cs`, JWT events are logged:
```csharp
OnAuthenticationFailed = context =>
{
    Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
    return Task.CompletedTask;
}
```

### Test with Swagger
1. Run `dotnet run`
2. Open `https://localhost:5170/swagger`
3. Use the "Authorize" button to add Bearer token
4. Test endpoints from UI

### Common Issues
- **403 Forbidden**: Ownership check failed. Verify `CreatedByUserId` in database matches JWT `sub` claim
- **401 Unauthorized**: JWT invalid. Check token expiration and configuration
- **404 Not Found**: Check soft delete filter (add `&& !IsDeleted`)
- **Build errors**: Run `dotnet clean && dotnet restore && dotnet build`

---

## MCP Servers (Model Context Protocol)

For enhanced IDE-like capabilities in Copilot sessions, consider configuring these MCP servers:

### Recommended Setup
1. **postgres-mcp** - Inspect database schema directly
   - Query tables, columns, indexes
   - Useful when modifying DbContext or migrations

2. **dotnet-cli** - Execute dotnet commands
   - Build, test, run migrations from Copilot
   - Check build errors inline

3. **git** - Repository analysis
   - View commit history, diffs, branches
   - Track changes across multiple files

### Configuration
Add to your IDE's MCP configuration (e.g., `.mcp.json` or through Claude Desktop config):
```json
{
  "mcpServers": {
    "postgres": {
      "command": "npx",
      "args": ["@modelcontextprotocol/server-postgres"],
      "env": {
        "DATABASE_URL": "postgresql://user:password@host:5432/auth_db_lx1b"
      }
    }
  }
}
```

---

## References

- **README_INDEX.md** - Overview of ownership authorization implementation
- **TROUBLESHOOTING_RUNTIME.md** - Common runtime errors and fixes
- **Implementation documentation** - IMPLEMENTATION_GUIDE.md, OWNERSHIP_AUTHORIZATION_SUMMARY.md
