using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Sharpnado.Tasks
{
    public partial class TaskMonitor : TaskMonitorBase
    {
        public static readonly ITaskMonitor NotStartedTask = new NotStartedTask();

        /// <summary>
        /// Callback called when the task successfully completed.
        /// </summary>
        private readonly Action<ITaskMonitor> _whenSuccessfullyCompleted;

        /// <inheritdoc />
        internal TaskMonitor(
            Task task,
            Action<ITaskMonitor> whenCanceled = null,
            Action<ITaskMonitor> whenFaulted = null,
            Action<ITaskMonitor> whenCompleted = null,
            Action<ITaskMonitor> whenSuccessfullyCompleted = null,
            string name = null,
            bool inNewTask = false,
            bool isHot = false,
            Action<string, Exception> errorHandler = null)
            : base(task, whenCanceled, whenFaulted, whenCompleted, name, inNewTask, isHot, errorHandler)
        {
            _whenSuccessfullyCompleted = whenSuccessfullyCompleted;

            if (isHot)
            {
                TaskCompleted = MonitorTaskAsync(task);
            }
        }

        protected override bool HasCallbacks => base.HasCallbacks || _whenSuccessfullyCompleted != null;

        /// <summary>
        /// Creates a new task monitor watching the specified task.
        /// </summary>
        public static TaskMonitor Create(
            Task task,
            Action<ITaskMonitor> whenCompleted = null,
            Action<ITaskMonitor> whenFaulted = null,
            Action<ITaskMonitor> whenSuccessfullyCompleted = null,
            string name = null)
        {
            return new TaskMonitor(
                task,
                whenCompleted: whenCompleted,
                whenFaulted: whenFaulted,
                whenSuccessfullyCompleted: whenSuccessfullyCompleted,
                name: name,
                isHot: true);
        }

        /// <summary>
        /// Creates a new task monitor watching the specified task.
        /// </summary>
        public static TaskMonitor Create(
            Func<Task> task,
            Action<ITaskMonitor> whenCompleted = null,
            Action<ITaskMonitor> whenFaulted = null,
            Action<ITaskMonitor> whenSuccessfullyCompleted = null,
            string name = null)
        {
            return new TaskMonitor(
                task(),
                whenCompleted: whenCompleted,
                whenFaulted: whenFaulted,
                whenSuccessfullyCompleted: whenSuccessfullyCompleted,
                name: name,
                isHot: true);
        }

        protected override void OnSuccessfullyCompleted()
        {
            try
            {
                _whenSuccessfullyCompleted?.Invoke(this);
            }
            catch (Exception exception)
            {
                ErrorHandler?.Invoke("Error while calling the WhenSuccessfullyCompleted callback", exception);
            }
        }
    }

    public class NotStartedTask : ITaskMonitor
    {
        public Task Task { get; }

        public Task TaskCompleted { get; }

        public TaskStatus Status => TaskStatus.Created;

        public bool IsNotStarted => true;

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