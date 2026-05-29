using MediatR;
using Login.Api.Infrastructure.Data;
using Login.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Login.Api.Features.Users.Update;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly LoginDbContext _context;

    public UpdateUserCommandHandler(LoginDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        ValidateInput(request);

        // Get the user to update
        var user = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Id == request.UserId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if user is inactive
        if (!user.IsActive)
        {
            throw new InvalidOperationException("Cannot update inactive user");
        }

        // Authorization: Check if requester can update this user
        AuthorizeUpdate(request, user);

        // Update email if provided and different
        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            // Check for duplicate email
            var existingEmail = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email already exists");
            }
            user.Email = request.Email;
        }

        // Update name fields if provided
        if (!string.IsNullOrEmpty(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            user.LastName = request.LastName;
        }

        // Update password if provided
        if (!string.IsNullOrEmpty(request.Password))
        {
            if (request.Password.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters");
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Return response
        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return new UpdateUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = roleNames,
            UpdatedAt = user.UpdatedAt ?? DateTime.UtcNow
        };
    }

    private void ValidateInput(UpdateUserCommand request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required");
        }

        if (request.FirstName != null && string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ArgumentException("FirstName cannot be empty");
        }

        if (request.LastName != null && string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("LastName cannot be empty");
        }

        if (!string.IsNullOrEmpty(request.Email) && !IsValidEmail(request.Email))
        {
            throw new ArgumentException("Invalid email format");
        }
    }

    private void AuthorizeUpdate(UpdateUserCommand request, User user)
    {
        // SuperAdmin can update anyone
        if (request.RequesterRoles.Contains("SuperAdmin"))
        {
            return;
        }

        // DistributorAdmin can only update themselves
        if (request.RequesterRoles.Contains("DistributorAdmin") && request.RequesterId != user.Id)
        {
            throw new UnauthorizedAccessException("DistributorAdmin can only update their own profile");
        }

        // Other roles can only update themselves
        if (request.RequesterId != user.Id)
        {
            throw new UnauthorizedAccessException("Cannot update other user's profile");
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
}
