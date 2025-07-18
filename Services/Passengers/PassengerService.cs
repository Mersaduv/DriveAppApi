using DriveApp.Data;
using DriveApp.DTOs.Passengers;
using DriveApp.Enums;
using DriveApp.Models.Passengers;
using DriveApp.Models.Users;
using DriveApp.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DriveApp.Services.Passengers;

public class PassengerService : BaseService, IPassengerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public PassengerService(
        AppDbContext dbContext, 
        ILogger<PassengerService> logger,
        IHttpContextAccessor httpContextAccessor) : base(dbContext, logger)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<PassengerDto> GetPassengerByIdAsync(Guid id)
    {
        var passenger = await _dbContext.Passengers
            .Include(p => p.User)
            .Include(p => p.FavoriteLocations)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            
        if (passenger == null)
        {
            throw new KeyNotFoundException($"Passenger with ID {id} not found");
        }
        
        return passenger.ToDto();
    }
    
    public async Task<IEnumerable<PassengerDto>> GetAllPassengersAsync()
    {
        var passengers = await _dbContext.Passengers
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .ToListAsync();
            
        return passengers.Select(p => p.ToDto());
    }
    
    public async Task<PassengerDto> CreatePassengerAsync(PassengerRegistrationDto passengerDto)
    {
        // First, check if phone number already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == passengerDto.PhoneNumber);
            
        User user;
        if (existingUser == null)
        {
            // Create a new user
            user = new User
            {
                PhoneNumber = passengerDto.PhoneNumber,
                FullName = passengerDto.FullName,
                Email = passengerDto.Email,
                IsActive = true,
                IsPhoneVerified = false  // Should be verified through a verification process
            };
            
            await _dbContext.Users.AddAsync(user);
        }
        else
        {
            // Use existing user
            user = existingUser;
            // Update missing user info if provided in registration
            bool userUpdated = false;
            if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(passengerDto.FullName))
            {
                user.FullName = passengerDto.FullName;
                userUpdated = true;
            }

            if (string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(passengerDto.Email))
            {
                user.Email = passengerDto.Email;
                userUpdated = true;
            }

            if (userUpdated)
            {
                _dbContext.Users.Update(user);
            }
            
            // Check if user already has a passenger profile
            var existingPassenger = await _dbContext.Passengers
                .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
                
            if (existingPassenger != null)
            {
                throw new InvalidOperationException($"User already has a passenger profile with ID {existingPassenger.Id}");
            }
        }
        
        // Create passenger profile
        var passenger = new Passenger
        {
            UserId = user.Id,
            PreferredPaymentMethod = passengerDto.PreferredPaymentMethod,
            Rating = 0,
            TotalTrips = 0
        };
        
        await _dbContext.Passengers.AddAsync(passenger);
        
        // Assign the Passenger role to user
        var passengerRole = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.RoleType == UserRoleType.Passenger);
            
        if (passengerRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = passengerRole.Id,
                AssignedAt = DateTime.UtcNow
            };
            
            await _dbContext.UserRoles.AddAsync(userRole);
        }
        else
        {
            _logger.LogWarning("Passenger role not found in the system");
        }
        
        await _dbContext.SaveChangesAsync();
        
        // Get the created passenger with all related data
        var createdPassenger = await _dbContext.Passengers
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == passenger.Id);
            
        return createdPassenger!.ToDto();
    }
    
    public async Task<PassengerDto> UpdatePassengerAsync(Guid id, UpdatePassengerDto passengerDto)
    {
        var passenger = await _dbContext.Passengers
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            
        if (passenger == null)
        {
            throw new KeyNotFoundException($"Passenger with ID {id} not found");
        }
        
        // Update passenger properties
        if (passengerDto.PreferredPaymentMethod != null)
        {
            passenger.PreferredPaymentMethod = passengerDto.PreferredPaymentMethod;
        }
        
        passenger.UpdatedAt = DateTime.UtcNow;
        passenger.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return passenger.ToDto();
    }
    
    public async Task<bool> DeletePassengerAsync(Guid id)
    {
        var passenger = await _dbContext.Passengers
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            
        if (passenger == null)
        {
            throw new KeyNotFoundException($"Passenger with ID {id} not found");
        }
        
        passenger.IsDeleted = true;
        passenger.UpdatedAt = DateTime.UtcNow;
        passenger.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<PassengerDto> GetPassengerByUserIdAsync(Guid userId)
    {
        var passenger = await _dbContext.Passengers
            .Include(p => p.User)
            .Include(p => p.FavoriteLocations)
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
            
        if (passenger == null)
        {
            throw new KeyNotFoundException($"Passenger with User ID {userId} not found");
        }
        
        return passenger.ToDto();
    }
    
    public async Task<IEnumerable<PassengerFavoriteLocationDto>> GetFavoriteLocationsAsync(Guid passengerId)
    {
        var passengerExists = await _dbContext.Passengers
            .AnyAsync(p => p.Id == passengerId && !p.IsDeleted);
            
        if (!passengerExists)
        {
            throw new KeyNotFoundException($"Passenger with ID {passengerId} not found");
        }
        
        var locations = await _dbContext.PassengerFavoriteLocations
            .Where(l => l.PassengerId == passengerId && !l.IsDeleted)
            .ToListAsync();
            
        return locations.Select(l => l.ToDto());
    }
    
    public async Task<PassengerFavoriteLocationDto> AddFavoriteLocationAsync(Guid passengerId, CreateFavoriteLocationDto locationDto)
    {
        var passengerExists = await _dbContext.Passengers
            .AnyAsync(p => p.Id == passengerId && !p.IsDeleted);
            
        if (!passengerExists)
        {
            throw new KeyNotFoundException($"Passenger with ID {passengerId} not found");
        }
        
        var location = new PassengerFavoriteLocation
        {
            PassengerId = passengerId,
            Name = locationDto.Name,
            Address = locationDto.Address,
            Latitude = locationDto.Latitude,
            Longitude = locationDto.Longitude,
            CreatedBy = GetCurrentUserName()
        };
        
        await _dbContext.PassengerFavoriteLocations.AddAsync(location);
        await _dbContext.SaveChangesAsync();
        
        return location.ToDto();
    }
    
    public async Task<PassengerFavoriteLocationDto> UpdateFavoriteLocationAsync(Guid locationId, UpdateFavoriteLocationDto locationDto)
    {
        var location = await _dbContext.PassengerFavoriteLocations
            .FirstOrDefaultAsync(l => l.Id == locationId && !l.IsDeleted);
            
        if (location == null)
        {
            throw new KeyNotFoundException($"Favorite location with ID {locationId} not found");
        }
        
        // Update properties
        if (locationDto.Name != null) location.Name = locationDto.Name;
        if (locationDto.Address != null) location.Address = locationDto.Address;
        if (locationDto.Latitude.HasValue) location.Latitude = locationDto.Latitude.Value;
        if (locationDto.Longitude.HasValue) location.Longitude = locationDto.Longitude.Value;
        
        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return location.ToDto();
    }
    
    public async Task<bool> DeleteFavoriteLocationAsync(Guid locationId)
    {
        var location = await _dbContext.PassengerFavoriteLocations
            .FirstOrDefaultAsync(l => l.Id == locationId && !l.IsDeleted);
            
        if (location == null)
        {
            throw new KeyNotFoundException($"Favorite location with ID {locationId} not found");
        }
        
        location.IsDeleted = true;
        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedBy = GetCurrentUserName();
        
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