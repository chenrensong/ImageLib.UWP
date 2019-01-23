using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System.Threading;
namespace ImageLib.Schedulers
{
    // Provides a task scheduler that ensures a maximum concurrency level while  
    // running on top of the thread pool. 
    // https://msdn.microsoft.com/en-us/library/ee789351.aspx?f=255&MSPPError=-2147217396
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;
        // The list of tasks to be executed  
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
        // The maximum concurrency level allowed by this scheduler. 
        private readonly int _maxDegreeOfParallelism;
        // Indicates whether the scheduler is currently processing work items. 
        private int _delegatesQueuedOrRunning;
        private WorkItemPriority _workItemPriority;
        private bool _fDisableInlining;
        // Gets the maximum concurrency level supported by this scheduler.  
        public sealed override int MaximumConcurrencyLevel
        {
            get
            {
                return this._maxDegreeOfParallelism;
            }
        }
        // Creates a new instance with the specified degree of parallelism. 
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism, WorkItemPriority priority, bool fDisableInlining = false)
        {
            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            }
            this._maxDegreeOfParallelism = maxDegreeOfParallelism;
            this._workItemPriority = priority;
            this._fDisableInlining = fDisableInlining;
        }
        // Queues a task to the scheduler.  
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough  
            // delegates currently queued or running to process tasks, schedule another.  
            lock (this._tasks)
            {
                this._tasks.AddLast(task);
                if (this._delegatesQueuedOrRunning < this._maxDegreeOfParallelism)
                {
                    this._delegatesQueuedOrRunning++;
                    this.NotifyThreadPoolOfPendingWork();
                }
            }
        }
        // Inform the ThreadPool that there's work to be executed for this scheduler.  
        private async void NotifyThreadPoolOfPendingWork()
        {
            await ThreadPool.RunAsync(delegate (IAsyncAction workItem)
            {
                // Note that the current thread is now processing work items. 
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue. 
                    while (true)
                    {
                        Task value;
                        lock (this._tasks)
                        {
                            // When there are no more items to be processed, 
                            // note that we're done processing, and get out. 
                            if (this._tasks.Count == 0)
                            {
                                this._delegatesQueuedOrRunning--;
                                break;
                            }
                            // Get the next item from the queue
                            value = this._tasks.First.Value;
                            this._tasks.RemoveFirst();
                        }
                        // Execute the task we pulled out of the queue 
                        base.TryExecuteTask(value);
                    }
                }
                finally
                {  // We're done processing items on the current thread 
                    _currentThreadIsProcessingItems = false;
                }
            }, this._workItemPriority);
        }

        // Attempts to execute the specified task on the current thread.  
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (this._fDisableInlining)
            {
                return false;
            }
            // If this thread isn't already processing a task, we don't support inlining 
            if (!_currentThreadIsProcessingItems)
            {
                return false;
            }
            // If the task was previously queued, remove it from the queue 
            if (taskWasPreviouslyQueued)
            {
                // Try to run the task.  
                this.TryDequeue(task);
            }
            return base.TryExecuteTask(task);
        }
        // Attempt to remove a previously scheduled task from the scheduler. 
        protected sealed override bool TryDequeue(Task task)
        {
            bool result;
            lock (this._tasks)
            {
                result = this._tasks.Remove(task);
            }
            return result;
        }
        // Gets an enumerable of the tasks currently scheduled on this scheduler. 
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool flag = false;
            IEnumerable<Task> result;
            try
            {
                Monitor.TryEnter(this._tasks, ref flag);
                if (!flag)
                {
                    throw new NotSupportedException();
                }
                result = this._tasks.ToArray<Task>();
            }
            finally
            {
                if (flag)
                {
                    Monitor.Exit(this._tasks);
                }
            }
            return result;
        }
    }
}
