using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sharpnado.Tasks.Tests
{
    public class TaskMonitorTest
    {
        [Fact]
        public async Task SimplestTest()
        {
            var monitor = TaskMonitor.Create(DelayAsync);

            await monitor.TaskCompleted;

            Assert.False(monitor.IsNotCompleted);
            Assert.True(monitor.IsSuccessfullyCompleted);
        }

        [Fact]
        public async Task NominalTest()
        {
            bool isCompleted = false;
            bool isSuccessfullyCompleted = false;
            bool isFaulted = false;
            string monitorName = "NominalTestTask";

            var monitor = TaskMonitor.Create(
                DelayAsync, 
                t => isCompleted = true, 
                t => isFaulted = true,
                t => isSuccessfullyCompleted = true, 
                name: monitorName);

            await monitor.TaskCompleted;

            Assert.True(isSuccessfullyCompleted && monitor.IsSuccessfullyCompleted);
            Assert.False(isFaulted || monitor.IsFaulted);
            Assert.False(monitor.IsCanceled);
            Assert.True(isCompleted && monitor.IsCompleted);
            Assert.Equal(monitorName, monitor.Name);
        }

        [Fact]
        public async Task NominalListTest()
        {
            bool isCompleted = false;
            bool isSuccessfullyCompleted = false;
            bool isFaulted = false;
            string monitorName = "NominalListTestTask";

            var monitor = TaskMonitor<List<string>>.Create(
                DelayListAsync, 
                t => isCompleted = true, 
                t => isFaulted = true,
                (t, r) =>
                {
                    isSuccessfullyCompleted = true;
                    Assert.Equal(3, r.Count);
                }, 
                name: monitorName);

            await monitor.TaskCompleted;

            Assert.True(isSuccessfullyCompleted && monitor.IsSuccessfullyCompleted);
            Assert.False(isFaulted || monitor.IsFaulted);
            Assert.False(monitor.IsCanceled);
            Assert.True(isCompleted && monitor.IsCompleted);
            Assert.Equal(monitorName, monitor.Name);
            Assert.Equal(3, monitor.Result.Count);
        }

        [Fact]
        public async Task NominalFaultTest()
        {
            bool isCompleted = false;
            bool isSuccessfullyCompleted = false;
            bool isFaulted = false;
            string monitorName = "NominalFaultTestTask";

            var monitor = TaskMonitor.Create(
                DelayFaultAsync, 
                t => isCompleted = true, 
                t => isFaulted = true,
                t => isSuccessfullyCompleted = true, 
                name: monitorName);

            await monitor.TaskCompleted;

            Assert.True(isCompleted && monitor.IsCompleted);
            Assert.True(isFaulted && monitor.IsFaulted);
            Assert.False(isSuccessfullyCompleted || monitor.IsSuccessfullyCompleted);
            Assert.False(monitor.IsCanceled);
            Assert.True(isCompleted && monitor.IsCompleted);
            Assert.Equal(monitorName, monitor.Name);
        }

        [Fact]
        public async Task NominalFaultFirstTest()
        {
            bool isCompleted = false;
            bool isSuccessfullyCompleted = false;
            bool isFaulted = false;
            string monitorName = "NominalFaultTestTask";

            var monitor = TaskMonitor.Create(
                DelayFaultFirstAsync, 
                t => isCompleted = true, 
                t => isFaulted = true,
                t => isSuccessfullyCompleted = true, 
                name: monitorName);

            await monitor.TaskCompleted;

            Assert.True(isCompleted && monitor.IsCompleted);
            Assert.True(isFaulted && monitor.IsFaulted);
            Assert.False(isSuccessfullyCompleted || monitor.IsSuccessfullyCompleted);
            Assert.False(monitor.IsCanceled);
            Assert.True(isCompleted && monitor.IsCompleted);
            Assert.Equal(monitorName, monitor.Name);
        }

        [Fact]
        public async Task InNewTaskTest()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            var monitor = TaskMonitor<int>.Create(
                DelayThreadIdAsync,
                inNewTask: true);

            await monitor.TaskCompleted;

            Assert.NotEqual(threadId, monitor.Result);
        }

        [Fact]
        public async Task UseMonitorAsDecoratedFaultTest()
        {
            TaskMonitorConfiguration.LogStatistics = true;
            TaskMonitorConfiguration.StatisticsHandler = TaskMonitorConfiguration.DefaultStatisticsTracer;

            var monitor = TaskMonitor.Create(DelayFaultAsync, name: "UseMonitorAsDecoratedFaultTest");

            try
            {
                await Assert.ThrowsAsync<Exception>(() => monitor.Task);
            }
            finally
            {
                TaskMonitorConfiguration.LogStatistics = false;
            }
        }

        [Fact]
        public async Task DisplayStatisticsTest()
        {
            TaskMonitorConfiguration.LogStatistics = true;
            await NominalTest();
            TaskMonitorConfiguration.LogStatistics = false;
        }

        [Fact]
        public async Task CustomStatisticsHandlerTest()
        {
            bool statsHandlerCalled = false;
            TaskMonitorConfiguration.LogStatistics = true;
            TaskMonitorConfiguration.StatisticsHandler = (t, time) =>
            {
                statsHandlerCalled = true;
                Assert.True(time.TotalMilliseconds > 0);
            };

            await NominalTest();

            Assert.True(statsHandlerCalled);
            TaskMonitorConfiguration.LogStatistics = false;
        }

        [Fact]
        public async Task CustomErrorHandlerTest()
        {
            bool errorHandlerCalled = false;
            TaskMonitorConfiguration.LogStatistics = true;
            TaskMonitorConfiguration.ErrorHandler = (t, m, e) =>
            {
                errorHandlerCalled = true;
            };

            await NominalFaultTest();

            Assert.True(errorHandlerCalled);
            TaskMonitorConfiguration.LogStatistics = false;
        }

        [Fact]
        public async Task CancelTaskTest()
        {
            var cts = new CancellationTokenSource();
            bool isFaulted = false;
            bool isCanceled = false;
            bool isSuccess = false;

            var monitor = new TaskMonitor.Builder(() => DelayCanceledAsync(cts.Token))
                .WithWhenCanceled(t => isCanceled = true)
                .WithWhenFaulted(t => isFaulted = true)
                .WithWhenSuccessfullyCompleted(t => isSuccess = true)
                .Build();

            cts.Cancel();
            monitor.Start();

            await monitor.TaskCompleted;

            Assert.True(isCanceled);
            Assert.False(isFaulted);
            Assert.False(isSuccess);
        }

        [Fact]
        public async Task ConsiderCanceledAsFaultedTest()
        {
            var cts = new CancellationTokenSource();
            bool isFaulted = false;
            bool isCanceled = false;
            bool isSuccess = false;

            var monitor = new TaskMonitor.Builder(() => DelayCanceledAsync(cts.Token))
                .WithWhenCanceled(t => isCanceled = true)
                .WithWhenFaulted(t => isFaulted = true)
                .WithWhenSuccessfullyCompleted(t => isSuccess = true)
                .WithConsiderCanceledAsFaulted(true)
                .Build();

            cts.Cancel();
            monitor.Start();

            await monitor.TaskCompleted;

            Assert.True(!isCanceled && monitor.IsCanceled);
            Assert.True(isFaulted && monitor.IsFaulted);
            Assert.False(isSuccess || monitor.IsSuccessfullyCompleted);
        }

        [Fact]
        public async Task ConsiderCanceledAsFaultedConfigurationTest()
        {
            var cts = new CancellationTokenSource();
            TaskMonitorConfiguration.ConsiderCanceledAsFaulted = true;
            bool isFaulted = false;
            bool isCanceled = false;
            bool isSuccess = false;

            var monitor = new TaskMonitor.Builder(() => DelayCanceledAsync(cts.Token))
                .WithWhenCanceled(t => isCanceled = true)
                .WithWhenFaulted(t => isFaulted = true)
                .WithWhenSuccessfullyCompleted(t => isSuccess = true)
                .Build();

            cts.Cancel();
            monitor.Start();

            await monitor.TaskCompleted;

            Assert.True(!isCanceled && monitor.IsCanceled);
            Assert.True(isFaulted && monitor.IsFaulted);
            Assert.False(isSuccess || monitor.IsSuccessfullyCompleted);

            TaskMonitorConfiguration.ConsiderCanceledAsFaulted = false;
        }

        private async Task DelayAsync()
        {
            await Task.Delay(200);
        }

        private async Task<int> DelayThreadIdAsync()
        {
            await Task.Delay(200);
            return Thread.CurrentThread.ManagedThreadId;
        }

        private Task DelayFaultFirstAsync()
        {
            throw new Exception("Fault");
            return Task.Delay(200);
        }

        private async Task DelayFaultAsync()
        {
            await Task.Delay(200);
            throw new Exception("Fault");
        }

        private async Task DelayCanceledAsync(CancellationToken token)
        {
            await Task.Delay(200, token);
        }

        private async Task<List<string>> DelayListAsync()
        {
            await Task.Delay(200);
            return new List<string> {"1", "2", "3"};
        }
    }
}
