using System;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Benchmarks
{
    public interface IDatabaseBenchmarks : IDisposable
    {
        Task Setup();
        void Cleanup();
        void Dispose();
        Task CreateUser();
        Task ReadUserByEmail();
        Task UpdateUser();
        Task DeleteUser();
        Task CreateService();
        Task ListServices();
        Task BulkCreateUsers();
        Task TransactionCommit();
    }
}
