// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotnetServiceScaffold.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotnetServiceScaffold.Tests.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ServiceScaffoldDbContext DbContext;

    protected IntegrationTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ServiceScaffoldDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new ServiceScaffoldDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DbContext.Dispose();
            _connection.Close();
        }
    }
}
