#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Generic repository interface for CRUD operations.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);

    Task<IEnumerable<T>> GetAllAsync();

    Task<T> AddAsync(T entity);

    Task<T> UpdateAsync(T entity);

    Task DeleteAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);

    Task SaveChangesAsync();
}
