using System;
using System.Threading.Tasks;

namespace Sharpnado.Tasks
{
    public abstract partial class TaskMonitorBase
    {
        public abstract class BuilderBase
        {
            protected Action<ITaskMonitor> WhenCompleted { get; set; }

            protected Action<ITaskMonitor> WhenCanceled { get; set; }

            protected Action<ITaskMonitor> WhenFaulted { get; set; }

            protected bool InANewTask { get; set; }

            protected string Name { get; set; }

            protected bool IsHot { get; set; }

            protected bool? ConsiderCanceledAsFaulted { get; set; }
        }
    }

    public partial class TaskMonitor
    {
        public class Builder : TaskMonitorBase.BuilderBase
        {
            public Builder(Func<Task> task)
            {
                TaskFunc = task;
            }

            protected Func<Task> TaskFunc { get; }

            protected Action<ITaskMonitor> WhenSuccessfullyCompleted { get; private set; }

            public Builder WithWhenCompleted(Action<ITaskMonitor> whenCompleted)
            {
                WhenCompleted = whenCompleted;
                return this;
            }

            public Builder WithWhenCanceled(Action<ITaskMonitor> whenCanceled)
            {
                WhenCanceled = whenCanceled;
                return this;
            }

            public Builder WithWhenFaulted(Action<ITaskMonitor> whenFaulted)
            {
                WhenFaulted = whenFaulted;
                return this;
            }

            public Builder WithConsiderCanceledAsFaulted(bool canceledAsFaulted)
            {
                ConsiderCanceledAsFaulted = canceledAsFaulted;
                return this;
            }

            public Builder WithName(string name)
            {
                Name = name;
                return this;
            }

            public Builder InNewTask()
            {
                InANewTask = true;
                return this;
            }

            public Builder WithIsHot()
            {
                IsHot = true;
                return this;
            }

            public Builder WithWhenSuccessfullyCompleted(Action<ITaskMonitor> whenSuccessfullyCompleted)
            {
                WhenSuccessfullyCompleted = whenSuccessfullyCompleted;
                return this;
            }

            public TaskMonitor Build()
            {
                return new (
                    null,
                    TaskFunc,
                    WhenCanceled,
                    WhenFaulted,
                    WhenCompleted,
                    WhenSuccessfullyCompleted,
                    Name,
                    InANewTask,
                    IsHot,
                    ConsiderCanceledAsFaulted);
            }
        }
    }

    public partial class TaskMonitor<TResult>
    {
        public class Builder : TaskMonitorBase.BuilderBase
        {
            public Builder(Func<Task<TResult>> taskFunc)
            {
                TaskFunc = taskFunc;
            }

            protected Func<Task<TResult>> TaskFunc { get; }

            protected Action<ITaskMonitor, TResult> WhenSuccessfullyCompleted { get; private set; }

            protected TResult DefaultResult { get; private set; }

            public Builder WithWhenCompleted(Action<ITaskMonitor> whenCompleted)
            {
                WhenCompleted = whenCompleted;
                return this;
            }

            public Builder WithWhenCanceled(Action<ITaskMonitor> whenCanceled)
            {
                WhenCanceled = whenCanceled;
                return this;
            }

            public Builder WithWhenFaulted(Action<ITaskMonitor> whenFaulted)
            {
                WhenFaulted = whenFaulted;
                return this;
            }

            public Builder WithName(string name)
            {
                Name = name;
                return this;
            }

            public Builder InNewTask()
            {
                InANewTask = true;
                return this;
            }

            public Builder WithIsHot()
            {
                IsHot = true;
                return this;
            }

            public Builder WithConsiderCanceledAsFaulted(bool canceledAsFaulted)
            {
                ConsiderCanceledAsFaulted = canceledAsFaulted;
                return this;
            }

            public Builder WithWhenSuccessfullyCompleted(Action<ITaskMonitor, TResult> whenSuccessfullyCompleted)
            {
                WhenSuccessfullyCompleted = whenSuccessfullyCompleted;
                return this;
            }

            public Builder WithDefaultResult(TResult defaultResult)
            {
                DefaultResult = defaultResult;
                return this;
            }

            public TaskMonitor<TResult> Build()
            {
                return new (
                    null,
                    TaskFunc,
                    DefaultResult,
                    WhenCanceled,
                    WhenFaulted,
                    WhenCompleted,
                    WhenSuccessfullyCompleted,
                    Name,
                    InANewTask,
                    IsHot,
                    ConsiderCanceledAsFaulted);
            }
        }
    }
}