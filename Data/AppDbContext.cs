using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DriveApp.Models.Users;
using DriveApp.Models.Drivers;
using DriveApp.Models.Passengers;
using DriveApp.Models.Trips;
using DriveApp.Models.System;

namespace DriveApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Users
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<PhoneVerification> PhoneVerifications { get; set; }

    // Drivers
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DriverLocation> DriverLocations { get; set; }

    // Passengers
    public DbSet<Passenger> Passengers { get; set; }
    public DbSet<PassengerFavoriteLocation> PassengerFavoriteLocations { get; set; }

    // Trips
    public DbSet<Trip> Trips { get; set; }
    public DbSet<TripLocation> TripLocations { get; set; }
    public DbSet<TripRating> TripRatings { get; set; }
    public DbSet<TripPayment> TripPayments { get; set; }

    // System
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<PriceConfiguration> PriceConfigurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships and indexes
        
        // Users
        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();
            
        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();
            
        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();
            
        // Drivers
        modelBuilder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithOne(u => u.Driver)
            .HasForeignKey<Driver>(d => d.UserId);
            
        // Passengers
        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.User)
            .WithOne(u => u.Passenger)
            .HasForeignKey<Passenger>(p => p.UserId);
            
        // Apply soft delete filter
        ApplySoftDeleteFilter(modelBuilder);
    }
    
    private void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    {
        // Apply soft delete filter to all entities that inherit from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Models.Core.BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, "IsDeleted");
                var falseConstant = Expression.Constant(false);
                var equalExpression = Expression.Equal(property, falseConstant);
                var lambda = Expression.Lambda(equalExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
} 