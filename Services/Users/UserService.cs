using Microsoft.EntityFrameworkCore;
using DriveApp.Data;
using DriveApp.DTOs.Users;
using DriveApp.Models.Users;
using DriveApp.Services.Helpers;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace DriveApp.Services.Users;

public class UserService : BaseService, IUserService
{
    private readonly IRoleService _roleService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UserService(
        AppDbContext dbContext, 
        IRoleService roleService, 
        ILogger<UserService> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(dbContext, logger)
    {
        _roleService = roleService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
            
        if (user == null)
            return null;
            
        return user.ToDto();
    }
    
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();
            
        return users.Select(u => u.ToDto());
    }
    
    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Check if phone number already exists
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == createUserDto.PhoneNumber);
        if (existingUser != null)
            throw new InvalidOperationException($"User with phone number {createUserDto.PhoneNumber} already exists");
            
        var user = createUserDto.ToEntity();
        
        await _dbContext.Users.AddAsync(user);
        
        // Add roles if provided
        if (createUserDto.RoleIds != null && createUserDto.RoleIds.Any())
        {
            foreach (var roleId in createUserDto.RoleIds)
            {
                var role = await _dbContext.Roles.FindAsync(roleId);
                if (role != null)
                {
                    await _dbContext.UserRoles.AddAsync(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }
        }
        
        await _dbContext.SaveChangesAsync();
        
        return await GetUserByIdAsync(user.Id);
    }
    
    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
            return null;
            
        user.UpdateFromDto(updateUserDto);
        
        // Update roles if provided
        if (updateUserDto.RoleIds != null)
        {
            // Get current roles
            var currentRoles = await _dbContext.UserRoles
                .Where(ur => ur.UserId == id)
                .ToListAsync();
            
            // Remove roles that are not in the new list
            foreach (var userRole in currentRoles)
            {
                if (!updateUserDto.RoleIds.Contains(userRole.RoleId))
                {
                    _dbContext.UserRoles.Remove(userRole);
                }
            }
            
            // Add new roles
            foreach (var roleId in updateUserDto.RoleIds)
            {
                if (!currentRoles.Any(ur => ur.RoleId == roleId))
                {
                    var role = await _dbContext.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        await _dbContext.UserRoles.AddAsync(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id,
                            AssignedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        
        return await GetUserByIdAsync(id);
    }
    
    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
            return false;
            
        // Soft delete
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<UserDto> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            
        if (user == null)
            return null;
            
        return user.ToDto();
    }
    
    public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);
            
        if (user == null)
            return Enumerable.Empty<RoleDto>();
            
        return user.UserRoles.Select(ur => ur.Role.ToDto());
    }
    
    public async Task<bool> AddRoleToUserAsync(Guid userId, Guid roleId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            return false;
            
        var role = await _dbContext.Roles.FindAsync(roleId);
        if (role == null)
            return false;
            
        // Check if user already has this role
        var existingUserRole = await _dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            
        if (existingUserRole != null)
            return true; // User already has this role
            
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };
        
        await _dbContext.UserRoles.AddAsync(userRole);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
    {
        var userRole = await _dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            
        if (userRole == null)
            return false;
            
        _dbContext.UserRoles.Remove(userRole);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> HasPermissionAsync(Guid userId, string permissionName)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);
            
        if (user == null)
            return false;
            
        return user.UserRoles
            .Any(ur => ur.Role.RolePermissions
                .Any(rp => rp.Permission.Name == permissionName));
    }
    
    // Authentication methods
    public async Task<string> RequestPhoneVerificationAsync(string phoneNumber)
    {
        // Validate phone number
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Phone number cannot be empty");
        }
        
        // Generate verification code
        var verificationCode = GenerateVerificationCode();
        
        // Create or get user
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            
        if (user == null)
        {
            user = new User
            {
                PhoneNumber = phoneNumber,
                IsPhoneVerified = false,
                IsActive = true,
                CreatedBy = "System"
            };
            
            await _dbContext.Users.AddAsync(user);
        }
        
        // Create verification record
        var verification = new PhoneVerification
        {
            UserId = user.Id,
            PhoneNumber = phoneNumber,
            VerificationCode = verificationCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10), // Code expires in 10 minutes
            IsUsed = false,
            CreatedBy = "System"
        };
        
        await _dbContext.PhoneVerifications.AddAsync(verification);
        await _dbContext.SaveChangesAsync();
        
        // TODO: Send SMS with verification code
        // This would integrate with an SMS provider like Twilio, AWS SNS, etc.
        
        _logger.LogInformation($"Verification code {verificationCode} generated for {phoneNumber}");
        
        return verification.Id.ToString();
    }
    
    public async Task<VerificationResultDto> VerifyPhoneNumberAsync(string phoneNumber, string verificationCode)
    {
        // Find the most recent verification request
        var verification = await _dbContext.PhoneVerifications
            .Where(v => 
                v.PhoneNumber == phoneNumber && 
                v.VerificationCode == verificationCode &&
                v.ExpiresAt > DateTime.UtcNow &&
                !v.IsUsed)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();
            
        if (verification == null)
        {
            return new VerificationResultDto
            {
                Success = false,
                Message = "Invalid or expired verification code"
            };
        }
        
        // Mark as used
        verification.IsUsed = true;
        verification.VerifiedAt = DateTime.UtcNow;
        
        // Find or create user
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == verification.UserId);
            
        if (user == null)
        {
            return new VerificationResultDto
            {
                Success = false,
                Message = "User not found"
            };
        }
        
        // Update user verification status
        bool isNewUser = !user.IsPhoneVerified;
        user.IsPhoneVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = "System";
        
        await _dbContext.SaveChangesAsync();
        
        // Generate tokens
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        
        return new VerificationResultDto
        {
            Success = true,
            Message = "Phone number verified successfully",
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            IsNewUser = isNewUser
        };
    }
    
    public async Task<LoginResultDto> LoginAsync(string phoneNumber, string verificationCode)
    {
        // Verify the phone number first
        var verificationResult = await VerifyPhoneNumberAsync(phoneNumber, verificationCode);
        
        if (!verificationResult.Success)
        {
            return new LoginResultDto
            {
                Success = false,
                Message = verificationResult.Message
            };
        }
        
        // Get user info
        var user = await GetUserByIdAsync(verificationResult.UserId);
        
        if (user == null)
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "User not found"
            };
        }
        
        // Update last login timestamp
        var userEntity = await _dbContext.Users.FindAsync(user.Id);
        userEntity.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        
        return new LoginResultDto
        {
            Success = true,
            Message = "Login successful",
            Token = verificationResult.Token,
            RefreshToken = verificationResult.RefreshToken,
            UserId = user.Id,
            User = user
        };
    }
    
    public async Task<TokenResultDto> RefreshTokenAsync(string refreshToken)
    {
        // In a production app, you would validate the refresh token against a stored token
        // For simplicity in this implementation, we'll validate the token signature and expiry
        
        try
        {
            var principal = ValidateToken(refreshToken);
            if (principal == null)
            {
                return new TokenResultDto
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }
            
            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return new TokenResultDto
                {
                    Success = false,
                    Message = "Invalid user ID in token"
                };
            }
            
            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive);
                
            if (user == null)
            {
                return new TokenResultDto
                {
                    Success = false,
                    Message = "User not found or inactive"
                };
            }
            
            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            
            return new TokenResultDto
            {
                Success = true,
                Token = newToken,
                RefreshToken = newRefreshToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error refreshing token: {ex.Message}");
            return new TokenResultDto
            {
                Success = false,
                Message = "Invalid token"
            };
        }
    }
    
    public async Task LogoutAsync(string token)
    {
        // In a production app, you would add the token to a blacklist or revoke it
        // For simplicity, we'll just log the logout
        _logger.LogInformation($"User logged out with token: {token}");
        await Task.CompletedTask;
    }
    
    public async Task<UserDto> GetUserFromTokenAsync(string token)
    {
        try
        {
            var principal = ValidateToken(token);
            if (principal == null)
            {
                return null;
            }
            
            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }
            
            return await GetUserByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating token: {ex.Message}");
            return null;
        }
    }
    
    // Helper methods
    private string GenerateVerificationCode()
    {
        // Generate a 6-digit code
        return new Random().Next(100000, 999999).ToString();
    }
    
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "default_secret_key_for_development_only_change_in_production";
        var issuer = jwtSettings["Issuer"] ?? "DriveApp";
        var audience = jwtSettings["Audience"] ?? "DriveApp_Users";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.PhoneNumber),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim("IsPhoneVerified", user.IsPhoneVerified.ToString())
        };
        
        // Add roles as claims
        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private string GenerateRefreshToken()
    {
        // In a production app, you would generate a secure refresh token and store it
        // For simplicity, we'll just create a new JWT with longer expiry
        
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["RefreshSecretKey"] ?? jwtSettings["SecretKey"] ?? "default_refresh_key_change_in_production";
        var issuer = jwtSettings["Issuer"] ?? "DriveApp";
        var audience = jwtSettings["Audience"] ?? "DriveApp_Users";
        var expiryDays = int.Parse(jwtSettings["RefreshExpiryDays"] ?? "7");
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expiryDays),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private ClaimsPrincipal ValidateToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "default_secret_key_for_development_only_change_in_production";
        var issuer = jwtSettings["Issuer"] ?? "DriveApp";
        var audience = jwtSettings["Audience"] ?? "DriveApp_Users";
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        
        if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }
        
        return principal;
    }
} 