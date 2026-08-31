#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Generic repository implementation with standard CRUD operations.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
	protected internal readonly ServiceScaffoldDbContext _context;
	protected internal readonly DbSet<T> _dbSet;
	protected internal readonly ILogger<Repository<T>> _logger;

	public Repository(ServiceScaffoldDbContext context, ILogger<Repository<T>> logger)
	{
		_context = context;
		_dbSet = context.Set<T>();
		_logger = logger;
	}

	public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			_logger.LogDebug("Retrieving entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
			return await _dbSet.FindAsync(new object?[] { id }, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
			throw new DataAccessException($"Error retrieving entity of type {typeof(T).Name}", ex);
		}
	}

	public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			_logger.LogDebug("Retrieving all entities of type {EntityType}", typeof(T).Name);
			return await _dbSet.ToListAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(T).Name);
			throw new DataAccessException($"Error retrieving all entities of type {typeof(T).Name}", ex);
		}
	}

	public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(entity);
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			_logger.LogDebug("Adding new entity of type {EntityType}", typeof(T).Name);
			var entry = await _dbSet.AddAsync(entity, cancellationToken);
			await SaveChangesAsync();
			_logger.LogInformation("Entity of type {EntityType} added successfully", typeof(T).Name);
			return entry.Entity;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
			throw new DataAccessException($"Error adding entity of type {typeof(T).Name}", ex);
		}
	}

	public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(entity);
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			_logger.LogDebug("Updating entity of type {EntityType}", typeof(T).Name);
			_dbSet.Update(entity);
			await SaveChangesAsync();
			_logger.LogInformation("Entity of type {EntityType} updated successfully", typeof(T).Name);
			return entity;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
			throw new DataAccessException($"Error updating entity of type {typeof(T).Name}", ex);
		}
	}

	public virtual async Task DeleteAsync(Guid id)
	{
		try
		{
			_logger.LogDebug("Deleting entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
			var entity = await GetByIdAsync(id);
			if (entity is not null)
			{
				_dbSet.Remove(entity);
				await SaveChangesAsync();
				_logger.LogInformation("Entity of type {EntityType} with ID {Id} deleted successfully", typeof(T).Name, id);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
			throw new DataAccessException($"Error deleting entity of type {typeof(T).Name}", ex);
		}
	}

	public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			_logger.LogDebug("Checking existence of entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
			return await _dbSet.FindAsync(new object?[] { id }, cancellationToken) is not null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking existence of entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
			throw new DataAccessException($"Error checking existence of entity", ex);
		}
	}

	public virtual async Task SaveChangesAsync()
	{
		try
		{
			_logger.LogDebug("Saving changes to database for context {ContextType}", nameof(ServiceScaffoldDbContext));
			await _context.SaveChangesAsync();
			_logger.LogDebug("Changes saved successfully to database");
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogWarning(ex, "Concurrency conflict detected while saving changes");
			throw new DataAccessException("Concurrency conflict: the data has been modified by another user", ex);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error saving changes to the database");
			throw new DataAccessException("Error saving changes to the database", ex);
		}
	}
}
