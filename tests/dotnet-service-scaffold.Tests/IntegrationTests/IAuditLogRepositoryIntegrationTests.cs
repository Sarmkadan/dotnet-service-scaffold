using System;
using System.Threading.Tasks;

public interface IAuditLogRepositoryIntegrationTests
{
    Task AddAuditLog_ShouldAddAuditLogToDatabase();
    Task GetAuditLogById_ShouldReturnCorrectAuditLog();
    Task UpdateAuditLog_ShouldUpdateAuditLogInDatabase();
    Task DeleteAuditLog_ShouldRemoveAuditLogFromDatabase();
    Task GetAllAuditLogs_ShouldReturnAllAuditLogs();
    Task GetAuditLogByNonExistentId_ShouldReturnNull();
}