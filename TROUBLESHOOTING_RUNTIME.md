# Troubleshooting: Program Won't Run

## Common Issues & Solutions

### Issue 1: SDK Not Found Error
**Error Message:** `MSB4236: The SDK 'Microsoft.NET.Sdk.Web' specified could not be found`

**Causes:**
- .NET 9 SDK not installed
- .NET SDK in wrong location
- Project file corrupted

**Solutions:**

A) **Install .NET 9 SDK**
```
1. Download from: https://dotnet.microsoft.com/download/dotnet/9.0
2. Choose "SDK" (not Runtime)
3. Install and restart Visual Studio
```

B) **Check installed .NET versions**
```powershell
dotnet --list-sdks
# Should show: 9.x.x [path]
```

C) **Verify project file**
```
Check QuizSystem.Api.csproj first line:
? <Project Sdk="Microsoft.NET.Sdk.Web">
? <TargetFramework>net9.0</TargetFramework>
```

---

### Issue 2: Build Errors After Changes

**If you see compilation errors:**

A) **Check for missing using statements**
```csharp
// These should be at top of file:
using QuizSystem.Api.QuestionSystem.Domain.Common;  // For ForbiddenAccessException
using QuizSystem.Api.QuestionSystem.Application.Abstractions.Persistence;  // For ICurrentUserService
```

B) **Restore NuGet packages**
```powershell
cd QuizSystem.Api
dotnet restore
```

C) **Clean build**
```powershell
dotnet clean
dotnet build
```

---

### Issue 3: Database Connection Issues

**Error:** `Cannot connect to database` or SQL connection errors

**Solutions:**

A) **Check connection string** in `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QuizSystemDb;Trusted_Connection=true;"
  }
}
```

B) **Verify SQL Server is running**
```powershell
# Windows
Get-Service "MSSQLSERVER" | Select-Object Status
# Should show: Status Running

# Or check SQL Server Configuration Manager
```

C) **Apply pending migrations**
```powershell
# In Package Manager Console
Add-Migration AddAuditFieldsAndSoftDelete
Update-Database
```

---

### Issue 4: Port Already in Use

**Error:** `Address already in use` or `bind failed`

**Solutions:**

A) **Change port** in `Properties/launchSettings.json`
```json
"applicationUrl": "https://localhost:5171;http://localhost:5170"
// Change 5170 to different port (5171, 5172, etc.)
```

B) **Kill process using port**
```powershell
# Find process on port 5170
Get-NetTCPConnection -LocalPort 5170 | Select-Object OwningProcess
# Kill it (if PID is 1234)
Stop-Process -Id 1234 -Force
```

---

### Issue 5: Authentication/JWT Configuration

**Error:** `JwtBearer: Invalid configuration` or similar

**Solutions:**

A) **Check Program.cs JWT configuration**
```csharp
// Should have these in Program.cs:
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.Authority = "YOUR_AUTH_API";
        options.Audience = "YOUR_API";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key")),
            ValidateIssuer = true,
            ValidIssuer = "RealWorldAPI",
            ValidateAudience = true,
            ValidAudience = "http://localhost:5170"
        };
    });
```

B) **Check appsettings.json**
```json
{
  "Jwt": {
    "ValidIssuer": "RealWorldAPI",
    "ValidAudience": "http://localhost:5170",
    "Key": "your-256-bit-secret-key-here"
  }
}
```

---

### Issue 6: Dependency Injection Errors

**Error:** `Unable to resolve service` or `No service registered for`

**Cause:** Missing dependency registration

**Solution:** Check `Program.cs` has these registrations:
```csharp
// Authentication
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Handlers
builder.Services.AddScoped<CreateFolderHandler>();
builder.Services.AddScoped<UpdateFolderHandler>();
builder.Services.AddScoped<DeleteFolderHandler>();
builder.Services.AddScoped<GetFolderHandler>();
builder.Services.AddScoped<GetAllFolderHandler>();

builder.Services.AddScoped<CreateQuestionGroupHandler>();
builder.Services.AddScoped<UpdateQuestionGroupHandler>();
builder.Services.AddScoped<DeleteQuestionGroupHandler>();
builder.Services.AddScoped<GetQuestionGroupHandler>();

// Repositories
builder.Services.AddScoped<IFolderRepository, FolderRepository>();
builder.Services.AddScoped<IQuestionGroupRepository, QuestionGroupRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Security
builder.Services.AddScoped<OwnershipGuard>();

// HTTP Context
builder.Services.AddHttpContextAccessor();
```

---

### Issue 7: Entity Framework Migration Issues

**Error:** `No DbContext was found` or migration errors

**Solutions:**

A) **Create DbContext if missing**
```csharp
public class QuizDbContext : DbContext
{
    public DbSet<Folder> Folders { get; set; }
    public DbSet<QuestionGroup> QuestionGroups { get; set; }
    public DbSet<Question> Questions { get; set; }

    public QuizDbContext(DbContextOptions<QuizDbContext> options) 
        : base(options)
    {
    }
}
```

B) **Register in Program.cs**
```csharp
builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

C) **Add migration**
```powershell
# In Package Manager Console
Add-Migration AddAuditFieldsAndSoftDelete
Update-Database
```

---

## Step-by-Step Troubleshooting

**Step 1: Check Environment**
```powershell
dotnet --version
dotnet --list-sdks
```
Expected: Shows .NET 9.x

**Step 2: Restore & Build**
```powershell
cd QuizSystem.Api
dotnet restore
dotnet build
```

**Step 3: Check for errors**
- If build succeeds ? Go to Step 4
- If build fails ? Read error message, check that section above

**Step 4: Run application**
```powershell
dotnet run
```

**Step 5: Test endpoints**
```bash
# Try to access API
curl http://localhost:5170/api/folders
# Should return 401 Unauthorized (no JWT token)
# Or the data if authenticated
```

---

## If Still Having Issues

Please provide:

1. **Exact error message** (copy-paste the full error)
2. **How you're running it** (Visual Studio F5, `dotnet run`, etc.)
3. **Which command fails first** (build, run, etc.)
4. **Screenshots** of error messages if possible

Then I can provide specific help!

---

## Quick Command Reference

```powershell
# Navigate to project
cd C:\Users\HP 14\QuizWebApp\QuizSystem.Api

# Check .NET version
dotnet --version

# List installed SDKs
dotnet --list-sdks

# Restore packages
dotnet restore

# Clean build
dotnet clean

# Build project
dotnet build

# Run project
dotnet run

# Run migrations
dotnet ef migrations add AddAuditFieldsAndSoftDelete
dotnet ef database update

# View specific error details
dotnet build --verbosity detailed
```

