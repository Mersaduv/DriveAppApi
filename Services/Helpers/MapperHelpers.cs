using DriveApp.Models.Users;
using DriveApp.Models.Drivers;
using DriveApp.Models.Passengers;
using DriveApp.Models.Trips;
using DriveApp.Models.System;
using DriveApp.DTOs.Users;
using DriveApp.DTOs.Drivers;
using DriveApp.DTOs.Passengers;
using DriveApp.DTOs.Trips;
using DriveApp.DTOs.System;

namespace DriveApp.Services.Helpers;

public static class MapperHelpers
{
    #region User Mappers
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            PhoneNumber = user.PhoneNumber,
            IsPhoneVerified = user.IsPhoneVerified,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
        };
    }
    
    public static User ToEntity(this CreateUserDto dto)
    {
        return new User
        {
            PhoneNumber = dto.PhoneNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth
        };
    }
    
    public static void UpdateFromDto(this User user, UpdateUserDto dto)
    {
        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth;
        if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
        
        user.UpdatedAt = DateTime.UtcNow;
    }
    
    public static RoleDto ToDto(this Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            RoleType = role.RoleType,
            IsSystemRole = role.IsSystemRole,
            Permissions = role.RolePermissions?.Select(rp => rp.Permission.Name).ToList() ?? new List<string>()
        };
    }
    
    public static Role ToEntity(this CreateRoleDto dto)
    {
        return new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            RoleType = dto.RoleType
        };
    }
    
    public static void UpdateFromDto(this Role role, UpdateRoleDto dto)
    {
        if (dto.Name != null) role.Name = dto.Name;
        if (dto.Description != null) role.Description = dto.Description;
        
        role.UpdatedAt = DateTime.UtcNow;
    }
    #endregion
    
    #region Driver Mappers
    public static DriverDto ToDto(this Driver driver)
    {
        return new DriverDto
        {
            Id = driver.Id,
            UserId = driver.UserId,
            NationalCardNumber = driver.NationalCardNumber,
            FullAddress = driver.FullAddress,
            DateOfBirth = driver.DateOfBirth,
            Status = driver.Status,
            Rating = driver.Rating,
            TotalTrips = driver.TotalTrips,
            IsOnline = driver.IsOnline,
            LastLocationUpdate = driver.LastLocationUpdate,
            CurrentLatitude = driver.CurrentLatitude,
            CurrentLongitude = driver.CurrentLongitude,
            ApprovedAt = driver.ApprovedAt,
            User = driver.User?.ToDto(),
            Vehicles = driver.Vehicles?.Select(v => v.ToDto()).ToList()
        };
    }
    
    public static Driver ToEntity(this DriverRegistrationDto dto, Guid userId)
    {
        return new Driver
        {
            UserId = userId,
            NationalCardNumber = dto.NationalCardNumber,
            FullAddress = dto.FullAddress,
            DateOfBirth = dto.DateOfBirth,
            Status = Enums.DriverStatus.Pending
        };
    }
    
    public static void UpdateStatus(this Driver driver, UpdateDriverStatusDto dto, string updatedBy)
    {
        driver.Status = dto.Status;
        if (dto.Status == Enums.DriverStatus.Rejected)
        {
            driver.RejectionReason = dto.RejectionReason;
        }
        else if (dto.Status == Enums.DriverStatus.Approved)
        {
            driver.ApprovedAt = DateTime.UtcNow;
            driver.ApprovedBy = updatedBy;
        }
        
        driver.UpdatedAt = DateTime.UtcNow;
        driver.UpdatedBy = updatedBy;
    }
    
    public static VehicleDto ToDto(this Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            DriverId = vehicle.DriverId,
            VehicleType = vehicle.VehicleType,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Color = vehicle.Color,
            PlateNumber = vehicle.PlateNumber,
            IsActive = vehicle.IsActive,
            IsVerified = vehicle.IsVerified,
            Documents = vehicle.Documents?.Select(d => d.ToDto()).ToList()
        };
    }
    
    public static Vehicle ToEntity(this CreateVehicleDto dto, Guid driverId)
    {
        return new Vehicle
        {
            DriverId = driverId,
            VehicleType = dto.VehicleType,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            Color = dto.Color,
            PlateNumber = dto.PlateNumber
        };
    }
    
    public static void UpdateFromDto(this Vehicle vehicle, UpdateVehicleDto dto)
    {
        if (dto.VehicleType.HasValue) vehicle.VehicleType = dto.VehicleType.Value;
        if (dto.Make != null) vehicle.Make = dto.Make;
        if (dto.Model != null) vehicle.Model = dto.Model;
        if (dto.Year != null) vehicle.Year = dto.Year;
        if (dto.Color != null) vehicle.Color = dto.Color;
        if (dto.PlateNumber != null) vehicle.PlateNumber = dto.PlateNumber;
        if (dto.IsActive.HasValue) vehicle.IsActive = dto.IsActive.Value;
        if (dto.IsVerified.HasValue) vehicle.IsVerified = dto.IsVerified.Value;
        
        vehicle.UpdatedAt = DateTime.UtcNow;
    }
    
    public static DocumentDto ToDto(this Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            DriverId = document.DriverId,
            VehicleId = document.VehicleId,
            DocumentType = document.DocumentType,
            FilePath = document.FilePath,
            OriginalFileName = document.OriginalFileName,
            FileSize = document.FileSize,
            MimeType = document.MimeType,
            IsVerified = document.IsVerified,
            VerifiedAt = document.VerifiedAt,
            VerifiedBy = document.VerifiedBy,
            RejectionReason = document.RejectionReason
        };
    }
    
    public static Document ToEntity(this UploadDocumentDto dto, string filePath, string originalFileName, long fileSize, string mimeType)
    {
        return new Document
        {
            DriverId = dto.DriverId,
            VehicleId = dto.VehicleId,
            DocumentType = dto.DocumentType,
            FilePath = filePath,
            OriginalFileName = originalFileName,
            FileSize = fileSize,
            MimeType = mimeType,
            IsVerified = false
        };
    }
    
    public static void UpdateVerification(this Document document, DocumentVerificationDto dto, string verifiedBy)
    {
        document.IsVerified = dto.IsVerified;
        if (dto.IsVerified)
        {
            document.VerifiedAt = DateTime.UtcNow;
            document.VerifiedBy = verifiedBy;
            document.RejectionReason = null;
        }
        else
        {
            document.RejectionReason = dto.RejectionReason;
            document.VerifiedAt = null;
            document.VerifiedBy = null;
        }
        
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = verifiedBy;
    }
    #endregion
    
    #region Passenger Mappers
    public static PassengerDto ToDto(this Passenger passenger)
    {
        return new PassengerDto
        {
            Id = passenger.Id,
            UserId = passenger.UserId,
            Rating = passenger.Rating,
            TotalTrips = passenger.TotalTrips,
            PreferredPaymentMethod = passenger.PreferredPaymentMethod,
            User = passenger.User?.ToDto(),
            FavoriteLocations = passenger.FavoriteLocations?.Select(fl => fl.ToDto()).ToList()
        };
    }
    
    public static Passenger ToEntity(this PassengerRegistrationDto dto, Guid userId)
    {
        return new Passenger
        {
            UserId = userId,
            PreferredPaymentMethod = dto.PreferredPaymentMethod
        };
    }
    
    public static void UpdateFromDto(this Passenger passenger, UpdatePassengerDto dto)
    {
        if (dto.PreferredPaymentMethod != null)
            passenger.PreferredPaymentMethod = dto.PreferredPaymentMethod;
        
        passenger.UpdatedAt = DateTime.UtcNow;
    }
    
    public static PassengerFavoriteLocationDto ToDto(this PassengerFavoriteLocation location)
    {
        return new PassengerFavoriteLocationDto
        {
            Id = location.Id,
            PassengerId = location.PassengerId,
            Name = location.Name,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude
        };
    }
    
    public static PassengerFavoriteLocation ToEntity(this CreateFavoriteLocationDto dto, Guid passengerId)
    {
        return new PassengerFavoriteLocation
        {
            PassengerId = passengerId,
            Name = dto.Name,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };
    }
    
    public static void UpdateFromDto(this PassengerFavoriteLocation location, UpdateFavoriteLocationDto dto)
    {
        if (dto.Name != null) location.Name = dto.Name;
        if (dto.Address != null) location.Address = dto.Address;
        if (dto.Latitude.HasValue) location.Latitude = dto.Latitude.Value;
        if (dto.Longitude.HasValue) location.Longitude = dto.Longitude.Value;
        
        location.UpdatedAt = DateTime.UtcNow;
    }
    #endregion
    
    #region Trip Mappers
    public static TripDto ToDto(this Trip trip)
    {
        return new TripDto
        {
            Id = trip.Id,
            PassengerId = trip.PassengerId,
            DriverId = trip.DriverId,
            VehicleId = trip.VehicleId,
            TripCode = trip.TripCode,
            
            OriginAddress = trip.OriginAddress,
            OriginLatitude = trip.OriginLatitude,
            OriginLongitude = trip.OriginLongitude,
            
            DestinationAddress = trip.DestinationAddress,
            DestinationLatitude = trip.DestinationLatitude,
            DestinationLongitude = trip.DestinationLongitude,
            
            Status = trip.Status,
            RequestedVehicleType = trip.RequestedVehicleType,
            EstimatedPrice = trip.EstimatedPrice,
            FinalPrice = trip.FinalPrice,
            Distance = trip.Distance,
            EstimatedDuration = trip.EstimatedDuration,
            ActualDuration = trip.ActualDuration,
            
            RequestedAt = trip.RequestedAt,
            AcceptedAt = trip.AcceptedAt,
            DriverArrivedAt = trip.DriverArrivedAt,
            StartedAt = trip.StartedAt,
            CompletedAt = trip.CompletedAt,
            CancelledAt = trip.CancelledAt,
            
            PassengerNotes = trip.PassengerNotes,
            CancellationReason = trip.CancellationReason,
            CancelledBy = trip.CancelledBy,
            
            Passenger = trip.Passenger?.ToDto(),
            Driver = trip.Driver?.ToDto(),
            Vehicle = trip.Vehicle?.ToDto(),
            Rating = trip.Rating?.ToDto(),
            Payment = trip.Payment?.ToDto()
        };
    }
    
    public static Trip ToEntity(this RequestTripDto dto, Guid passengerId, decimal estimatedPrice, string tripCode)
    {
        return new Trip
        {
            PassengerId = passengerId,
            TripCode = tripCode,
            
            OriginAddress = dto.OriginAddress,
            OriginLatitude = dto.OriginLatitude,
            OriginLongitude = dto.OriginLongitude,
            
            DestinationAddress = dto.DestinationAddress,
            DestinationLatitude = dto.DestinationLatitude,
            DestinationLongitude = dto.DestinationLongitude,
            
            Status = Enums.TripStatus.Requested,
            RequestedVehicleType = dto.RequestedVehicleType,
            EstimatedPrice = estimatedPrice,
            
            RequestedAt = DateTime.UtcNow,
            
            PassengerNotes = dto.PassengerNotes
        };
    }
    
    public static TripRatingDto ToDto(this TripRating rating)
    {
        return new TripRatingDto
        {
            Id = rating.Id,
            TripId = rating.TripId,
            PassengerRating = rating.PassengerRating,
            DriverRating = rating.DriverRating,
            PassengerComment = rating.PassengerComment,
            DriverComment = rating.DriverComment,
            RatedAt = rating.RatedAt
        };
    }
    
    public static TripRating ToEntity(this SubmitPassengerRatingDto dto, Guid tripId)
    {
        return new TripRating
        {
            TripId = tripId,
            PassengerRating = dto.Rating,
            PassengerComment = dto.Comment,
            RatedAt = DateTime.UtcNow
        };
    }
    
    public static void AddDriverRating(this TripRating rating, SubmitDriverRatingDto dto)
    {
        rating.DriverRating = dto.Rating;
        rating.DriverComment = dto.Comment;
        rating.UpdatedAt = DateTime.UtcNow;
    }
    
    public static TripPaymentDto ToDto(this TripPayment payment)
    {
        return new TripPaymentDto
        {
            Id = payment.Id,
            TripId = payment.TripId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentReference = payment.PaymentReference,
            IsPaid = payment.IsPaid,
            PaidAt = payment.PaidAt
        };
    }
    
    public static TripPayment ToEntity(this ProcessPaymentDto dto, Guid tripId, decimal amount)
    {
        return new TripPayment
        {
            TripId = tripId,
            Amount = amount,
            PaymentMethod = dto.PaymentMethod,
            PaymentReference = dto.PaymentReference,
            IsPaid = true,
            PaidAt = DateTime.UtcNow
        };
    }
    #endregion
    
    #region System Mappers
    public static SystemSettingDto ToDto(this SystemSetting setting)
    {
        return new SystemSettingDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            Category = setting.Category,
            IsPublic = setting.IsPublic
        };
    }
    
    public static SystemSetting ToEntity(this CreateSystemSettingDto dto)
    {
        return new SystemSetting
        {
            Key = dto.Key,
            Value = dto.Value,
            Description = dto.Description,
            Category = dto.Category,
            IsPublic = dto.IsPublic
        };
    }
    
    public static void UpdateFromDto(this SystemSetting setting, UpdateSystemSettingDto dto)
    {
        if (dto.Value != null) setting.Value = dto.Value;
        if (dto.Description != null) setting.Description = dto.Description;
        if (dto.Category != null) setting.Category = dto.Category;
        if (dto.IsPublic.HasValue) setting.IsPublic = dto.IsPublic.Value;
        
        setting.UpdatedAt = DateTime.UtcNow;
    }
    
    public static PriceConfigurationDto ToDto(this PriceConfiguration config)
    {
        return new PriceConfigurationDto
        {
            Id = config.Id,
            VehicleType = config.VehicleType,
            BasePrice = config.BasePrice,
            PricePerKm = config.PricePerKm,
            PricePerMinute = config.PricePerMinute,
            MinimumPrice = config.MinimumPrice,
            IsActive = config.IsActive,
            Description = config.Description
        };
    }
    
    public static PriceConfiguration ToEntity(this CreatePriceConfigurationDto dto)
    {
        return new PriceConfiguration
        {
            VehicleType = dto.VehicleType,
            BasePrice = dto.BasePrice,
            PricePerKm = dto.PricePerKm,
            PricePerMinute = dto.PricePerMinute,
            MinimumPrice = dto.MinimumPrice,
            IsActive = dto.IsActive,
            Description = dto.Description
        };
    }
    
    public static void UpdateFromDto(this PriceConfiguration config, UpdatePriceConfigurationDto dto)
    {
        if (dto.BasePrice.HasValue) config.BasePrice = dto.BasePrice.Value;
        if (dto.PricePerKm.HasValue) config.PricePerKm = dto.PricePerKm.Value;
        if (dto.PricePerMinute.HasValue) config.PricePerMinute = dto.PricePerMinute.Value;
        if (dto.MinimumPrice.HasValue) config.MinimumPrice = dto.MinimumPrice.Value;
        if (dto.IsActive.HasValue) config.IsActive = dto.IsActive.Value;
        if (dto.Description != null) config.Description = dto.Description;
        
        config.UpdatedAt = DateTime.UtcNow;
    }
    #endregion
} 