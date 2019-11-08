using System;
using System.Threading.Tasks;

namespace Sharpnado.Tasks
{
    /// <summary>
    /// Watches a task returning a result and calls callbacks when the task completes.
    /// </summary>
    public interface ITaskMonitor<out TResult> : ITaskMonitor
    {
        /// <summary>
        /// Gets the result of the task. Returns the "default result" value specified in the constructor if the task has not yet completed successfully.
        /// </summary>
        TResult Result { get; }
    }

    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    public interface ITaskMonitor
    {
        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        Task Task { get; }

        /// <summary>
        /// Gets a task that completes successfully when <see cref="Task"/> completes (successfully, faulted, or canceled). This property never changes and is never <c>null</c>.
        /// </summary>
        Task TaskCompleted { get; }

        /// <summary>
        /// Gets the current task status.
        /// </summary>
        TaskStatus Status { get; }

        /// <summary>
        /// Gets whether the task has started.
        /// </summary>
        bool IsNotStarted { get; }

        /// <summary>
        /// Gets whether the task has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets whether the task is busy (not completed).
        /// </summary>
        bool IsNotCompleted { get; }

        /// <summary>
        /// Gets whether the task has completed successfully.
        /// </summary>
        bool IsSuccessfullyCompleted { get; }

        /// <summary>
        /// Gets whether the task has been canceled.
        /// </summary>
        bool IsCanceled { get; }

        /// <summary>
        /// Gets whether the task has faulted.
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        /// The name given to the task by the user.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Whether or not the user gave a name to this monitor.
        /// </summary>
        bool HasName { get; }

        /// <summary>
        /// Gets the wrapped faulting exception for the task. Returns <c>null</c> if the task is not faulted.
        /// </summary>
        AggregateException Exception { get; }

        /// <summary>
        /// Gets the original faulting exception for the task. Returns <c>null</c> if the task is not faulted.
        /// </summary>
        Exception InnerException { get; }

        /// <summary>
        /// Gets the error message for the original faulting exception for the task. Returns <c>null</c> if the task is not faulted.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// In case of a cold task, we start it manually.
        /// </summary>
        void Start();

        /// <summary>
        /// Cancels the callbacks: the task will execute till the end, but none of the callbacks will be invoked.
        /// </summary>
        void CancelCallbacks();
    }
}