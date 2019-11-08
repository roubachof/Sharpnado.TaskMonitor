using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sharpnado.Tasks
{
    public static class TaskMonitorConfiguration
    {
        public static bool LogStatistics { get; set; } = false;

        public static bool ConsiderCanceledAsFaulted { get; set; } = false;

        public static Action<string, Exception> ErrorHandler { get; set; } = DefaultExceptionTracer;

        public static Action<ITaskMonitor, TimeSpan> StatisticsHandler { get; set; } = DefaultStatisticsTracer;

        public static bool HasDefaultErrorHandler => ErrorHandler == DefaultExceptionTracer;

        public static bool HasDefaultStatsTracer => StatisticsHandler == DefaultStatisticsTracer;

        public static void DefaultExceptionTracer(string message, Exception exception)
        {
            Trace.WriteLine(ExceptionTracerFormatter(message, exception));
        }

        public static string ExceptionTracerFormatter(string message, Exception exception)
            => $"TaskMonitor|ERROR|{Thread.CurrentThread.ManagedThreadId:000}|{message}{Environment.NewLine}Exception:{exception}{Environment.NewLine}";

        public static void DefaultStatisticsTracer(ITaskMonitor taskMonitor, TimeSpan taskExecutionTime)
        {
            Trace.WriteLine(StatisticsTracerFormatter(taskMonitor, taskExecutionTime));
        }

        public static string StatisticsTracerFormatter(ITaskMonitor taskMonitor, TimeSpan taskExecutionTime)
        {
            var statisticsBuilder = new StringBuilder($"TaskMonitor|STATS|{Thread.CurrentThread.ManagedThreadId:000}|");
            statisticsBuilder.Append(taskMonitor);
            statisticsBuilder.Append(", Executed in ");
            statisticsBuilder.Append(taskExecutionTime.TotalMilliseconds);
            statisticsBuilder.Append(" ms");
            return statisticsBuilder.ToString();
        }
    }
}
