// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.IntegrationTests;

public class ApiKeyRepositoryIntegrationTests : IntegrationTestBase
{
    private readonly ApiKeyRepository _apiKeyRepository;

    public ApiKeyRepositoryIntegrationTests()
    {
        _apiKeyRepository = new ApiKeyRepository(DbContext);
    }

    [Fact]
    public async Task AddApiKey_ShouldAddApiKeyToDatabase()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = "hashedkey1",
            KeyPrefix = "prefix1",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _apiKeyRepository.AddAsync(apiKey);
        await DbContext.SaveChangesAsync();

        // Assert
        var retrievedApiKey = await DbContext.ApiKeys.FindAsync(apiKey.Id);
        retrievedApiKey.Should().NotBeNull();
        retrievedApiKey!.KeyPrefix.Should().Be("prefix1");
    }

    [Fact]
    public async Task GetApiKeyById_ShouldReturnCorrectApiKey()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = "hashedkey2",
            KeyPrefix = "prefix2",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _apiKeyRepository.AddAsync(apiKey);
        await DbContext.SaveChangesAsync();

        // Act
        var retrievedApiKey = await _apiKeyRepository.GetByIdAsync(apiKey.Id);

        // Assert
        retrievedApiKey.Should().NotBeNull();
        retrievedApiKey!.KeyPrefix.Should().Be("prefix2");
    }

    [Fact]
    public async Task UpdateApiKey_ShouldUpdateApiKeyInDatabase()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = "hashedkey3",
            KeyPrefix = "prefix3",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _apiKeyRepository.AddAsync(apiKey);
        await DbContext.SaveChangesAsync();

        // Detach the entity
        DbContext.Entry(apiKey).State = EntityState.Detached;

        apiKey.KeyPrefix = "updatedPrefix3";
        apiKey.UpdatedAt = DateTime.UtcNow.AddHours(1);

        // Act
        _apiKeyRepository.Update(apiKey);
        await DbContext.SaveChangesAsync();

        // Assert
        var updatedApiKey = await DbContext.ApiKeys.FindAsync(apiKey.Id);
        updatedApiKey.Should().NotBeNull();
        updatedApiKey!.KeyPrefix.Should().Be("updatedPrefix3");
    }

    [Fact]
    public async Task DeleteApiKey_ShouldRemoveApiKeyFromDatabase()
    {
        // Arrange
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = "hashedkey4",
            KeyPrefix = "prefix4",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _apiKeyRepository.AddAsync(apiKey);
        await DbContext.SaveChangesAsync();

        // Act
        _apiKeyRepository.Delete(apiKey);
        await DbContext.SaveChangesAsync();

        // Assert
        var deletedApiKey = await DbContext.ApiKeys.FindAsync(apiKey.Id);
        deletedApiKey.Should().BeNull();
    }

    [Fact]
    public async Task GetAllApiKeys_ShouldReturnAllApiKeys()
    {
        // Arrange
        await _apiKeyRepository.AddAsync(new ApiKey { Id = Guid.NewGuid(), KeyHash = "hash5", KeyPrefix = "prefix5", UserId = Guid.NewGuid(), ExpiresAt = DateTime.UtcNow.AddYears(1), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await _apiKeyRepository.AddAsync(new ApiKey { Id = Guid.NewGuid(), KeyHash = "hash6", KeyPrefix = "prefix6", UserId = Guid.NewGuid(), ExpiresAt = DateTime.UtcNow.AddYears(1), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await DbContext.SaveChangesAsync();

        // Act
        var apiKeys = await _apiKeyRepository.GetAllAsync();

        // Assert
        apiKeys.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    public async Task GetApiKeyByNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var retrievedApiKey = await _apiKeyRepository.GetByIdAsync(nonExistentId);

        // Assert
        retrievedApiKey.Should().BeNull();
    }

    [Fact]
    public async Task AddApiKey_WithExistingPrefix_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var apiKey1 = new ApiKey { Id = Guid.NewGuid(), KeyHash = "hash7", KeyPrefix = "commonPrefix", UserId = userId, ExpiresAt = DateTime.UtcNow.AddYears(1), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var apiKey2 = new ApiKey { Id = Guid.NewGuid(), KeyHash = "hash8", KeyPrefix = "commonPrefix", UserId = userId, ExpiresAt = DateTime.UtcNow.AddYears(1), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

        await _apiKeyRepository.AddAsync(apiKey1);
        await DbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(async () =>
        {
            await _apiKeyRepository.AddAsync(apiKey2);
            await DbContext.SaveChangesAsync();
        });
    }
}
