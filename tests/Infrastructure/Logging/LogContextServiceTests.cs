using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.Logging
{
    /// <summary>
    /// Tests for <see cref="LogContextService"/> ensuring proper scope propagation,
    /// disposal, nesting, and async isolation.
    /// </summary>
    public class LogContextServiceTests
    {
        private readonly CollectingSink _sink = new();

        public LogContextServiceTests()
        {
            // Configure a logger that enriches from LogContext and writes to the in‑memory sink.
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Sink(_sink)
                .CreateLogger();
        }

        private void ResetSink()
        {
            _sink.Events.Clear();
            Log.CloseAndFlush();
        }

        [Fact]
        public void PushedProperty_IsVisibleWithinScope()
        {
            ResetSink();
            var service = new LogContextService();
            service.AddProperty("TestKey", "TestValue");

            using (service.PushProperties())
            {
                Log.Information("Inside scope");
            }

            var logEvent = Assert.Single(_sink.Events);
            Assert.True(logEvent.Properties.ContainsKey("TestKey"));
            Assert.Equal("\"TestValue\"", logEvent.Properties["TestKey"]?.ToString());
        }

        [Fact]
        public void Property_IsNotVisibleAfterScopeEnds()
        {
            ResetSink();
            var service = new LogContextService();
            service.AddProperty("LeakKey", "ShouldNotLeak");

            using (service.PushProperties())
            {
                Log.Information("Inside");
            }

            Log.Information("Outside");

            Assert.Equal(2, _sink.Events.Count);
            var inside = _sink.Events[0];
            var outside = _sink.Events[1];

            Assert.True(inside.Properties.ContainsKey("LeakKey"));
            Assert.False(outside.Properties.ContainsKey("LeakKey"));
        }

        [Fact]
        public void NestedScopes_ShadowAndRestoreProperties()
        {
            ResetSink();
            var service = new LogContextService();
            service.AddProperty("Key", "Outer");

            using (service.PushProperties())
            {
                Log.Information("Outer event");

                service.AddProperty("Key", "Inner");
                using (service.PushProperties())
                {
                    Log.Information("Inner event");
                }

                Log.Information("After inner");
            }

            Assert.Equal(3, _sink.Events.Count);
            var outer = _sink.Events[0];
            var inner = _sink.Events[1];
            var afterInner = _sink.Events[2];

            Assert.Equal("\"Outer\"", outer.Properties["Key"]?.ToString());
            Assert.Equal("\"Inner\"", inner.Properties["Key"]?.ToString());
            Assert.Equal("\"Outer\"", afterInner.Properties["Key"]?.ToString());
        }

        [Fact]
        public async Task AsyncFlows_IsolatedBetweenConcurrentCalls()
        {
            ResetSink();
            var service = new LogContextService();

            var task1 = Task.Run(async () =>
            {
                service.AddProperty("TaskId", "A");
                using (service.PushProperties())
                {
                    await Task.Delay(10);
                    Log.Information("Task A");
                }
            });

            var task2 = Task.Run(async () =>
            {
                service.AddProperty("TaskId", "B");
                using (service.PushProperties())
                {
                    await Task.Delay(5);
                    Log.Information("Task B");
                }
            });

            await Task.WhenAll(task1, task2);

            Assert.Equal(2, _sink.Events.Count);
            var messages = _sink.Events.Select(e => e.MessageTemplate.Text).ToArray();
            Assert.Contains("Task A", messages);
            Assert.Contains("Task B", messages);

            var taskAEvent = _sink.Events.First(e => e.MessageTemplate.Text == "Task A");
            var taskBEvent = _sink.Events.First(e => e.MessageTemplate.Text == "Task B");

            Assert.Equal("\"A\"", taskAEvent.Properties["TaskId"]?.ToString());
            Assert.Equal("\"B\"", taskBEvent.Properties["TaskId"]?.ToString());
        }

        /// <summary>
        /// Simple in‑memory sink that collects emitted <see cref="LogEvent"/> instances.
        /// </summary>
        private sealed class CollectingSink : ILogEventSink
        {
            private readonly object _lock = new();

            public List<LogEvent> Events { get; } = new();

            public void Emit(LogEvent logEvent)
            {
                lock (_lock)
                {
                    Events.Add(logEvent);
                }
            }
        }
    }
}
