using DriveApp.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveApp.Services;

public abstract class BaseService
{
    protected readonly AppDbContext _dbContext;
    protected readonly ILogger _logger;

    protected BaseService(AppDbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    protected async Task<T?> GetByIdAsync<T>(Guid id) where T : class
    {
        try
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting {typeof(T).Name} with ID {id}");
            throw;
        }
    }
    
    protected async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
    {
        try
        {
            return await _dbContext.Set<T>().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting all {typeof(T).Name}");
            throw;
        }
    }
    
    protected async Task<T> AddAsync<T>(T entity) where T : class
    {
        try
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding {typeof(T).Name}");
            throw;
        }
    }
    
    protected async Task<T> UpdateAsync<T>(T entity) where T : class
    {
        try
        {
            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating {typeof(T).Name}");
            throw;
        }
    }
    
    protected async Task<bool> DeleteAsync<T>(T entity) where T : class
    {
        try
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting {typeof(T).Name}");
            throw;
        }
    }
    
    protected async Task<bool> SaveChangesAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            throw;
        }
    }
} 