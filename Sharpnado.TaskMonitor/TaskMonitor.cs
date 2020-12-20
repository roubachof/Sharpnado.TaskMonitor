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
            Task task = null,
            Func<Task> taskSource = null,
            Action<ITaskMonitor> whenCanceled = null,
            Action<ITaskMonitor> whenFaulted = null,
            Action<ITaskMonitor> whenCompleted = null,
            Action<ITaskMonitor> whenSuccessfullyCompleted = null,
            string name = null,
            bool inNewTask = false,
            bool isHot = false,
            bool? considerCanceledAsFaulted = null,
            Action<ITaskMonitor, string, Exception> errorHandler = null)
            : base(task, taskSource, whenCanceled, whenFaulted, whenCompleted, name, inNewTask, isHot, considerCanceledAsFaulted, errorHandler)
        {
            _whenSuccessfullyCompleted = whenSuccessfullyCompleted;

            if (isHot)
            {
                TaskCompleted = MonitorTaskAsync();
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
            bool isHot = true,
            string name = null,
            bool inNewTask = false)
        {
            return new (
                task,
                whenCompleted: whenCompleted,
                whenFaulted: whenFaulted,
                whenSuccessfullyCompleted: whenSuccessfullyCompleted,
                name: name,
                isHot: isHot,
                inNewTask: inNewTask);
        }

        /// <summary>
        /// Creates a new task monitor watching the specified task.
        /// </summary>
        public static TaskMonitor Create(
            Func<Task> taskSource,
            Action<ITaskMonitor> whenCompleted = null,
            Action<ITaskMonitor> whenFaulted = null,
            Action<ITaskMonitor> whenSuccessfullyCompleted = null,
            bool isHot = true,
            string name = null,
            bool inNewTask = false)
        {
            return new (
                taskSource: taskSource,
                whenCompleted: whenCompleted,
                whenFaulted: whenFaulted,
                whenSuccessfullyCompleted: whenSuccessfullyCompleted,
                name: name,
                isHot: isHot,
                inNewTask: inNewTask);
        }

        protected override void OnSuccessfullyCompleted()
        {
            try
            {
                _whenSuccessfullyCompleted?.Invoke(this);
            }
            catch (Exception exception)
            {
                ErrorHandler?.Invoke(this, "Error while calling the WhenSuccessfullyCompleted callback", exception);
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