#nullable enable
using System;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Benchmarks
{
    /// <summary>
    /// Extension methods for <see cref="DatabaseBenchmarks"/> to compose common benchmark sequences.
    /// </summary>
    public static class DatabaseBenchmarksExtensions
    {
        /// <summary>
        /// Runs a sequence of user benchmark operations: create, read, update, delete.
        /// </summary>
        /// <param name="dbBench">The database benchmarks instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbBench"/> is null.</exception>
        public static async Task RunUserBenchmarkSequence(this DatabaseBenchmarks dbBench)
        {
            ArgumentNullException.ThrowIfNull(dbBench);
            await dbBench.CreateUser();
            await dbBench.ReadUserByEmail();
            await dbBench.UpdateUser();
            await dbBench.DeleteUser();
        }

        /// <summary>
        /// Runs a sequence of service benchmark operations: create and list.
        /// </summary>
        /// <param name="dbBench">The database benchmarks instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbBench"/> is null.</exception>
        public static async Task RunServiceBenchmarkSequence(this DatabaseBenchmarks dbBench)
        {
            ArgumentNullException.ThrowIfNull(dbBench);
            await dbBench.CreateService();
            await dbBench.ListServices();
        }

        /// <summary>
        /// Runs a bulk user creation followed by a transaction commit.
        /// </summary>
        /// <param name="dbBench">The database benchmarks instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbBench"/> is null.</exception>
        public static async Task RunBulkAndTransaction(this DatabaseBenchmarks dbBench)
        {
            ArgumentNullException.ThrowIfNull(dbBench);
            await dbBench.BulkCreateUsers();
            await dbBench.TransactionCommit();
        }
    }
}