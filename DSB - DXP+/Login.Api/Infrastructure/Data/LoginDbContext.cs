using Login.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Login.Api.Infrastructure.Data;

public class LoginDbContext : DbContext
{
    public LoginDbContext(DbContextOptions<LoginDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Workshop> Workshops { get; set; }
    public DbSet<WorkshopSchedule> WorkshopSchedules { get; set; }
    public DbSet<WorkshopHoliday> WorkshopHolidays { get; set; }
    public DbSet<WorkshopBlackoutDate> WorkshopBlackoutDates { get; set; }
    public DbSet<Advisor> Advisors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).IsRequired();
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // UserRole (Many-to-Many) configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsRevoked).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Workshop configuration
        modelBuilder.Entity<Workshop>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Brand).IsRequired();
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(255);
                address.Property(a => a.City).HasMaxLength(100);
                address.Property(a => a.Region).HasMaxLength(100);
                address.Property(a => a.PostalCode).HasMaxLength(20);
                address.Property(a => a.Country).HasMaxLength(100);
            });
        });

        // WorkshopSchedule configuration
        modelBuilder.Entity<WorkshopSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Workshop)
                .WithMany(w => w.Schedules)
                .HasForeignKey(e => e.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkshopHoliday configuration
        modelBuilder.Entity<WorkshopHoliday>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Workshop)
                .WithMany(w => w.Holidays)
                .HasForeignKey(e => e.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkshopBlackoutDate configuration
        modelBuilder.Entity<WorkshopBlackoutDate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Workshop)
                .WithMany(w => w.BlackoutDates)
                .HasForeignKey(e => e.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Advisor configuration
        modelBuilder.Entity<Advisor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.WorkshopId).IsRequired();
            entity.Property(e => e.AssignedBrand).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.AvailableHoursPerDay).IsRequired();
            entity.HasOne(e => e.Workshop)
                .WithMany()
                .HasForeignKey(e => e.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default roles
        var superAdminRole = new Role 
        { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), 
            Name = "SuperAdmin",
            Description = "Platform-wide control" 
        };
        var distributorAdminRole = new Role 
        { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), 
            Name = "DistributorAdmin",
            Description = "Scoped operations by brand/region" 
        };
        var workshopUserRole = new Role 
        { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), 
            Name = "WorkshopUser",
            Description = "Read-only workshop access" 
        };

        modelBuilder.Entity<Role>().HasData(superAdminRole, distributorAdminRole, workshopUserRole);
    }
}
