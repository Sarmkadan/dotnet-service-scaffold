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
/// <typeparam name="T">The type of entity managed by the repository.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
	/// <summary>
	/// The database context used by the repository.
	/// </summary>
	protected internal readonly ServiceScaffoldDbContext _context;

	/// <summary>
	/// The set of entities managed by the repository.
	/// </summary>
	protected internal readonly DbSet<T> _dbSet;

	/// <summary>
	/// The logger used to record repository operations.
	/// </summary>
	protected internal readonly ILogger<Repository<T>> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="Repository{T}"/> class.
	/// </summary>
	/// <param name="context">The database context used to access persisted entities.</param>
	/// <param name="logger">The logger used to record repository operations.</param>
	public Repository(ServiceScaffoldDbContext context, ILogger<Repository<T>> logger)
	{
		_context = context;
		_dbSet = context.Set<T>();
		_logger = logger;
	}

	/// <summary>
	/// Retrieves an entity by its unique identifier.
	/// </summary>
	/// <param name="id">The unique identifier of the entity.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The matching entity, or <see langword="null"/> if no entity is found.</returns>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled before the operation starts.</exception>
	/// <exception cref="DataAccessException">An error occurs while retrieving the entity.</exception>
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

	/// <summary>
	/// Retrieves all entities in the repository.
	/// </summary>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>A collection containing all entities.</returns>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled before the operation starts.</exception>
	/// <exception cref="DataAccessException">An error occurs while retrieving the entities.</exception>
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

	/// <summary>
	/// Adds an entity to the repository and persists the change.
	/// </summary>
	/// <param name="entity">The entity to add.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The entity that was added.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null"/>.</exception>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled before the operation starts.</exception>
	/// <exception cref="DataAccessException">An error occurs while adding or persisting the entity.</exception>
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

	/// <summary>
	/// Updates an entity in the repository and persists the change.
	/// </summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The updated entity.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null"/>.</exception>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled before the operation starts.</exception>
	/// <exception cref="DataAccessException">An error occurs while updating or persisting the entity.</exception>
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

	/// <summary>
	/// Deletes the entity with the specified unique identifier, if it exists, and persists the change.
	/// </summary>
	/// <param name="id">The unique identifier of the entity to delete.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <exception cref="DataAccessException">An error occurs while retrieving, deleting, or persisting the entity.</exception>
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

	/// <summary>
	/// Determines whether an entity with the specified unique identifier exists.
	/// </summary>
	/// <param name="id">The unique identifier of the entity.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns><see langword="true"/> if the entity exists; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled before the operation starts.</exception>
	/// <exception cref="DataAccessException">An error occurs while checking for the entity.</exception>
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

	/// <summary>
	/// Persists pending changes to the database.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <exception cref="DataAccessException">A concurrency conflict or another error occurs while saving changes.</exception>
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
