using MediatR;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using System.Text.RegularExpressions;

namespace Login.Api.Features.Users.Create;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly LoginDbContext _context;

    public CreateUserCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validate inputs
        ValidateInput(request);

        // Check for duplicate email
        var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserRoles = new List<UserRole>()
        };

        // Assign roles if provided
        if (request.RoleIds?.Count > 0)
        {
            var roles = _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id))
                .ToList();

            if (roles.Count != request.RoleIds.Count)
            {
                throw new InvalidOperationException("Role not found");
            }

            foreach (var role in roles)
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    User = user,
                    Role = role
                });
            }
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Return response
        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return new CreateUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = roleNames,
            CreatedAt = user.CreatedAt
        };
    }

    private void ValidateInput(CreateUserCommand request)
    {
        // Email validation
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required");
        }

        if (!IsValidEmail(request.Email))
        {
            throw new ArgumentException("Invalid email format");
        }

        // FirstName validation
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ArgumentException("FirstName is required");
        }

        // LastName validation
        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("LastName is required");
        }

        // Password validation
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required");
        }

        if (request.Password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters");
        }

        if (!HasPasswordComplexity(request.Password))
        {
            throw new ArgumentException("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool HasPasswordComplexity(string password)
    {
        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigit = Regex.IsMatch(password, @"[0-9]");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+=\-\[\]{};:'""\\|,.<>\/?]");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }
}
