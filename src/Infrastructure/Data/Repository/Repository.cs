// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Generic repository implementation with standard CRUD operations.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ServiceScaffoldDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ServiceScaffoldDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Error retrieving entity of type {typeof(T).Name}", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Error retrieving all entities of type {typeof(T).Name}", ex);
        }
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            var entry = await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entry.Entity;
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Error adding entity of type {typeof(T).Name}", ex);
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        try
        {
            _dbSet.Update(entity);
            await SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Error updating entity of type {typeof(T).Name}", ex);
        }
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Error deleting entity of type {typeof(T).Name}", ex);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            return await _dbSet.FindAsync(id) != null;
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Error checking existence of entity", ex);
        }
    }

    public virtual async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new DataAccessException("Concurrency conflict: the data has been modified by another user", ex);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Error saving changes to the database", ex);
        }
    }
}
