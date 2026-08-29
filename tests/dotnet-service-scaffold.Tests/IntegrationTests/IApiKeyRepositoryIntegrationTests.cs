using System;
using System.Threading.Tasks;

/// <summary>
/// Interface for ApiKeyRepositoryIntegrationTests.
/// </summary>
public interface IApiKeyRepositoryIntegrationTests
{
    Task AddApiKey_ShouldAddApiKeyToDatabase();
    Task GetApiKeyById_ShouldReturnCorrectApiKey();
    Task UpdateApiKey_ShouldUpdateApiKeyInDatabase();
    Task DeleteApiKey_ShouldRemoveApiKeyFromDatabase();
    Task GetAllApiKeys_ShouldReturnAllApiKeys();
    Task GetApiKeyByNonExistentId_ShouldReturnNull();
    Task AddApiKey_WithExistingPrefix_ShouldThrowException();
}