using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Sharpnado.Tasks
{
    public partial class TaskMonitor<TResult> : TaskMonitorBase, ITaskMonitor<TResult>
    {
        public static readonly ITaskMonitor<TResult> NotStartedTask = new NotStartedTask<TResult>();

        /// <summary>
        /// The "result" of the task when it has not yet completed.
        /// </summary>
        private readonly TResult _defaultResult;

        /// <summary>
        /// Callback called when the task successfully completed.
        /// </summary>
        private readonly Action<ITaskMonitor, TResult> _whenSuccessfullyCompleted;

        /// <inheritdoc />
        internal TaskMonitor(
            Task<TResult> task,
            TResult defaultResult = default(TResult),
            Action<ITaskMonitor> whenCanceled = null,
            Action<ITaskMonitor> whenFaulted = null,
            Action<ITaskMonitor> whenCompleted = null,
            Action<ITaskMonitor, TResult> whenSuccessfullyCompleted = null,
            string name = null,
            bool inNewTask = false,
            bool isHot = false,
            bool? considerCanceledAsFaulted = null,
            Action<string, Exception> errorHandler = null)
            : base(task, whenCanceled, whenFaulted, whenCompleted, name, inNewTask, isHot, considerCanceledAsFaulted, errorHandler)
        {
            _defaultResult = defaultResult;
            _whenSuccessfullyCompleted = whenSuccessfullyCompleted;
            Task = task;

            if (isHot)
            {
                TaskCompleted = MonitorTaskAsync(task);
            }
        }

        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        public new Task<TResult> Task { get; }

        /// <summary>
        /// Gets the result of the task. Returns the "default result" value specified in the constructor if the task has not yet completed successfully. This property raises a notification when the task completes successfully.
        /// </summary>
        public TResult Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : _defaultResult;

        protected override bool HasCallbacks => base.HasCallbacks || _whenSuccessfullyCompleted != null;

        /// <summary>
        /// Creates a new task monitor watching the specified task.
        /// </summary>
        public static TaskMonitor<TResult> Create(
            Task<TResult> task,
            Action<ITaskMonitor> whenCompleted = null,
            Action<ITaskMonitor> whenFaulted = null,
            Action<ITaskMonitor, TResult> whenSuccessfullyCompleted = null,
            string name = null,
            bool isHot = true,
            TResult defaultResult = default(TResult))
        {
            return new TaskMonitor<TResult>(
                task,
                whenCompleted: whenCompleted,
                whenFaulted: whenFaulted,
                whenSuccessfullyCompleted: whenSuccessfullyCompleted,
                defaultResult: defaultResult,
                name: name,
                isHot: isHot);
        }

        /// <summary>
        /// Creates a new task monitor watching the specified task.
        /// </summary>
        public static TaskMonitor<TResult> Create(
            Func<Task<TResult>> task,
            Action<ITaskMonitor> whenCompleted = null,
            Action<ITaskMonitor> whenFaulted = null,
            Action<ITaskMonitor, TResult> whenSuccessfullyCompleted = null,
            string name = null,
            bool isHot = true,
            TResult defaultResult = default(TResult))
        {
            return new TaskMonitor<TResult>(
                task(),
                whenCompleted: whenCompleted,
                whenFaulted: whenFaulted,
                whenSuccessfullyCompleted: whenSuccessfullyCompleted,
                defaultResult: defaultResult,
                name: name,
                isHot: isHot);
        }

        protected override void OnSuccessfullyCompleted()
        {
            try
            {
                _whenSuccessfullyCompleted?.Invoke(this, Result);
            }
            catch (Exception exception)
            {
                ErrorHandler?.Invoke("Error while calling the WhenSuccessfullyCompleted callback", exception);
            }
        }
    }

    public class NotStartedTask<TResult> : ITaskMonitor<TResult>
    {
        public TResult Result => default(TResult);

        public Task Task { get; }

        public Task TaskCompleted { get; }

        public TaskStatus Status => TaskStatus.Created;

        public bool IsNotStarted => true;

        public bool IsRunning { get; }

        public bool IsCompleted { get; }

        public bool IsNotCompleted => true;

        public bool IsSuccessfullyCompleted { get; }

        public bool IsCanceled { get; }

        public bool IsFaulted { get; }

        public string Name { get; }

        public bool HasName { get; }

        public AggregateException Exception { get; }

        public Exception InnerException { get; }

        public string ErrorMessage { get; }

        public void Start()
        {
            throw new NotSupportedException();
        }

        public void CancelCallbacks()
        {
            throw new NotSupportedException();
        }
    }
}