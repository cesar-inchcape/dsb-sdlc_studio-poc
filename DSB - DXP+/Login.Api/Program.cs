using System.Text;
using Login.Api.Features.Auth;
using Login.Api.Features.Users;
using Login.Api.Features.Workshops;
using Login.Api.Features.Advisors;
using Login.Api.Features.Schedules;
using Login.Api.Infrastructure.Authorization;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DSB Login API",
        Version = "v1",
        Description = "Digital Service Booking - Authentication & Authorization API"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add DbContext
builder.Services.AddDbContext<LoginDbContext>(options =>
    options.UseInMemoryDatabase("LoginDb")); // For development; use SQL Server in production

// Add MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add JWT Token Generator
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? throw new InvalidOperationException("JWT Audience not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
        };
    });

builder.Services.AddAuthorization(options =>
{
    AuthorizationPolicies.RegisterPolicies(options);
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LoginDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Seed test user if not exists
    if (!dbContext.Users.Any(u => u.Email == "admin@dsb.cl"))
    {
        var adminRole = dbContext.Roles.FirstOrDefault(r => r.Name == "SuperAdmin");
        if (adminRole != null)
        {
            var adminUser = new Login.Api.Infrastructure.Data.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = "admin@dsb.cl",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(adminUser);
            
            dbContext.UserRoles.Add(new Login.Api.Infrastructure.Data.Entities.UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });
            
            dbContext.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map authentication endpoints
app.MapAuthEndpoints();

// Map user management endpoints
app.MapUserEndpoints();

// Map workshop management endpoints
app.MapWorkshopEndpoints();

// Map advisor management endpoints
app.MapAdvisorEndpoints();

// Map schedule management endpoints
app.MapScheduleEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }

