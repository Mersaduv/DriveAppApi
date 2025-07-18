using DriveApp.Data;
using DriveApp.DTOs.Drivers;
using DriveApp.Enums;
using DriveApp.Models.Drivers;
using DriveApp.Models.Users;
using DriveApp.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DriveApp.Services.Drivers;

public class DriverService : BaseService, IDriverService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public DriverService(
        AppDbContext dbContext, 
        ILogger<DriverService> logger,
        IHttpContextAccessor httpContextAccessor) : base(dbContext, logger)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<DriverDto> GetDriverByIdAsync(Guid id)
    {
        var driver = await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Vehicles)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found");
        }
        
        return driver.ToDto();
    }
    
    public async Task<IEnumerable<DriverDto>> GetAllDriversAsync()
    {
        var drivers = await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Vehicles)
            .Where(d => !d.IsDeleted)
            .ToListAsync();
            
        return drivers.Select(d => d.ToDto());
    }
    
    public async Task<IEnumerable<DriverDto>> GetDriversByStatusAsync(DriverStatus status)
    {
        var drivers = await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Vehicles)
            .Where(d => d.Status == status && !d.IsDeleted)
            .ToListAsync();
            
        return drivers.Select(d => d.ToDto());
    }
    
    public async Task<DriverDto> CreateDriverAsync(DriverRegistrationDto driverDto)
    {
        // First, check if phone number already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == driverDto.PhoneNumber);
            
        User user;
        if (existingUser == null)
        {
            // Create a new user
            user = new User
            {
                PhoneNumber = driverDto.PhoneNumber,
                FullName = driverDto.FullName,
                Email = driverDto.Email,
                DateOfBirth = driverDto.DateOfBirth,
                IsActive = true,
                IsPhoneVerified = false  // Should be verified through a verification process
            };
            
            await _dbContext.Users.AddAsync(user);
        }
        else
        {
            // Use existing user
            user = existingUser;
            
            // Check if user already has a driver profile
            var existingDriver = await _dbContext.Drivers
                .FirstOrDefaultAsync(d => d.UserId == user.Id && !d.IsDeleted);
                
            if (existingDriver != null)
            {
                throw new InvalidOperationException($"User already has a driver profile with ID {existingDriver.Id}");
            }
        }
        
        // Create driver profile
        var driver = new Driver
        {
            UserId = user.Id,
            NationalCardNumber = driverDto.NationalCardNumber,
            FullAddress = driverDto.FullAddress,
            DateOfBirth = driverDto.DateOfBirth,
            Status = DriverStatus.Pending
        };
        
        await _dbContext.Drivers.AddAsync(driver);
        
        // Assign the Driver role to user
        var driverRole = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.RoleType == UserRoleType.Driver);
            
        if (driverRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = driverRole.Id,
                AssignedAt = DateTime.UtcNow
            };
            
            await _dbContext.UserRoles.AddAsync(userRole);
        }
        else
        {
            _logger.LogWarning("Driver role not found in the system");
        }
        
        await _dbContext.SaveChangesAsync();
        
        // Get the created driver with all related data
        var createdDriver = await _dbContext.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == driver.Id);
            
        return createdDriver!.ToDto();
    }
    
    public async Task<DriverDto> UpdateDriverStatusAsync(Guid id, UpdateDriverStatusDto statusDto)
    {
        var driver = await _dbContext.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found");
        }
        
        var currentUserName = GetCurrentUserName();
        driver.UpdateStatus(statusDto, currentUserName);
        
        await _dbContext.SaveChangesAsync();
        
        return driver.ToDto();
    }
    
    public async Task<bool> DeleteDriverAsync(Guid id)
    {
        var driver = await _dbContext.Drivers
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found");
        }
        
        driver.IsDeleted = true;
        driver.UpdatedAt = DateTime.UtcNow;
        driver.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<DriverDto> UpdateDriverLocationAsync(Guid id, DriverLocationUpdateDto locationDto)
    {
        var driver = await _dbContext.Drivers
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found");
        }
        
        // Update current location
        driver.CurrentLatitude = locationDto.Latitude;
        driver.CurrentLongitude = locationDto.Longitude;
        driver.LastLocationUpdate = DateTime.UtcNow;
        
        // Add to location history
        var locationHistory = new DriverLocation
        {
            DriverId = driver.Id,
            Latitude = locationDto.Latitude,
            Longitude = locationDto.Longitude,
            Speed = locationDto.Speed,
            Heading = locationDto.Heading,
            Timestamp = DateTime.UtcNow
        };
        
        await _dbContext.DriverLocations.AddAsync(locationHistory);
        await _dbContext.SaveChangesAsync();
        
        return driver.ToDto();
    }
    
    public async Task<IEnumerable<DriverDto>> GetNearbyDriversAsync(double latitude, double longitude, double radiusKm)
    {
        // Using the Haversine formula for distance calculation
        // This is a simplified calculation that works for short distances
        const double earthRadiusKm = 6371.0;
        
        var drivers = await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Vehicles)
            .Where(d => 
                d.IsOnline &&
                d.Status == DriverStatus.Approved &&
                !d.IsDeleted &&
                d.CurrentLatitude.HasValue &&
                d.CurrentLongitude.HasValue &&
                d.LastLocationUpdate >= DateTime.UtcNow.AddMinutes(-5))
            .ToListAsync();
        
        // Filter drivers within the radius
        var nearbyDrivers = drivers.Where(d => 
        {
            var dLat = (d.CurrentLatitude!.Value - latitude) * (Math.PI / 180);
            var dLon = (d.CurrentLongitude!.Value - longitude) * (Math.PI / 180);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(latitude * (Math.PI / 180)) * Math.Cos(d.CurrentLatitude!.Value * (Math.PI / 180)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = earthRadiusKm * c;
            
            return distance <= radiusKm;
        });
        
        return nearbyDrivers.Select(d => d.ToDto());
    }
    
    public async Task<DriverDto> GetDriverByUserIdAsync(Guid userId)
    {
        var driver = await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Vehicles)
            .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);
            
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with User ID {userId} not found");
        }
        
        return driver.ToDto();
    }
    
    public async Task<bool> ToggleDriverOnlineStatusAsync(Guid id, bool isOnline)
    {
        var driver = await _dbContext.Drivers
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found");
        }
        
        // Can only set online status to true if driver is approved
        if (isOnline && driver.Status != DriverStatus.Approved)
        {
            throw new InvalidOperationException("Only approved drivers can go online");
        }
        
        driver.IsOnline = isOnline;
        driver.UpdatedAt = DateTime.UtcNow;
        driver.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    private string GetCurrentUserName()
    {
        if (_httpContextAccessor.HttpContext?.User?.Identity?.Name != null)
        {
            return _httpContextAccessor.HttpContext.User.Identity.Name;
        }
        
        return "System";
    }
} 