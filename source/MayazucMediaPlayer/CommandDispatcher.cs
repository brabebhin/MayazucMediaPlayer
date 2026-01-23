using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MayazucMediaPlayer
{
    /// <summary>
    /// A home brew message queue.
    /// This is used by the background media player to queue the events coming from media foundation
    /// and the user inputs in order to eliminate thread races
    /// 
    /// It provides several advantages:
    ///     It allows dispatching work that's not UI bound on background threads seamlessly
    ///     It works around the Dispatcher/Queue not having async/await support in run delegates
    ///     Provides some rate limiting (especially for user input) and bulkhead isolation in error scenarios.
    /// </summary>
    public partial class AsyncCommandDispatcher : IDisposable
    {
        Task backgroundThread = Task.CompletedTask;
        bool cancel = false;
        bool started = false;
        readonly AutoResetEvent cancelEvent = new AutoResetEvent(false);
        readonly BufferBlock<AsyncCommandDispatcherOperation> executionQueue = new BufferBlock<AsyncCommandDispatcherOperation>();

        public AsyncCommandDispatcher()
        {
            Start();
        }

        public void Start()
        {
            lock (executionQueue)
            {
                if (started) return;
                backgroundThread = Task.Factory.StartNew(async () =>
                {
                    started = true;
                    while (await executionQueue.OutputAvailableAsync())
                    {
                        if (executionQueue.TryReceive(out var actionToExecute))
                        {
                            try
                            {
                                await actionToExecute.Execute();
                            }
                            catch
                            {

                            }
                        }
                    }

                    cancelEvent.Set();
                }, TaskCreationOptions.LongRunning);
            }
        }

        public void Dispose()
        {
            cancel = true;
            executionQueue.Complete();
            cancelEvent.WaitOne();
            started = false;
        }

        public Task<CommandDispatcherOperationAsyncContext> EnqueueAsync(Action a)
        {
            var returnValue = new CommandDispatcherOperationAsyncContext();
            if (cancel) return Task.FromResult(returnValue.SetTimedOut(true));

            var e = new AsyncCommandDispatcherOperation(a);
            return SafeQueueOperation(e);
        }

        public Task<CommandDispatcherOperationAsyncContext> EnqueueAsync(Func<object> actionWithReturnValue)
        {
            var returnValue = new CommandDispatcherOperationAsyncContext();
            if (cancel) return Task.FromResult(returnValue.SetTimedOut(true));

            var e = new AsyncCommandDispatcherOperation((Func<object>)actionWithReturnValue);
            return SafeQueueOperation(e);
        }

        private Task<CommandDispatcherOperationAsyncContext> SafeQueueOperation(AsyncCommandDispatcherOperation e)
        {
            executionQueue.Post(e);
            if (backgroundThread.Status == TaskStatus.RanToCompletion)
            {
                if (!cancel)
                {
                    Start();
                }
            }
            return e.GetAsyncOperation();
        }

        public Task<CommandDispatcherOperationAsyncContext> EnqueueAsync(Func<Task> a)
        {
            var returnValue = new CommandDispatcherOperationAsyncContext();

            if (cancel) return Task.FromResult(returnValue.SetTimedOut(true));

            var e = new AsyncCommandDispatcherOperation(a);
            return SafeQueueOperation(e);
        }

        public Task<CommandDispatcherOperationAsyncContext> EnqueueAsync(Func<Task<object>> a)
        {
            var returnValue = new CommandDispatcherOperationAsyncContext();
            if (cancel) return Task.FromResult(returnValue.SetTimedOut(true));

            var e = new AsyncCommandDispatcherOperation(a);
            return SafeQueueOperation(e);
        }

        private class AsyncCommandDispatcherOperation
        {
            readonly Func<object>? syncActionWithReturnValue;
            readonly Func<Task<object>>? asyncActionWithReturnValue;
            readonly Func<Task>? asyncActionWithoutReturnValue;
            readonly TaskCompletionSource<CommandDispatcherOperationAsyncContext> executionTask = new TaskCompletionSource<CommandDispatcherOperationAsyncContext>();

            public AsyncCommandDispatcherOperation(Action a)
            {
                syncActionWithReturnValue = new Func<CommandDispatcherOperationAsyncContext>(() => { a(); return new CommandDispatcherOperationAsyncContext(); });
            }

            public AsyncCommandDispatcherOperation(Func<object> a)
            {
                syncActionWithReturnValue = a;
            }

            public AsyncCommandDispatcherOperation(Func<Task> a)
            {
                asyncActionWithoutReturnValue = a;
            }

            public AsyncCommandDispatcherOperation(Func<Task<object>> a)
            {
                asyncActionWithReturnValue = a;
            }

            public async Task Execute()
            {
                var returnValue = new CommandDispatcherOperationAsyncContext();
                try
                {
                    var result = syncActionWithReturnValue?.Invoke();
                    if (asyncActionWithReturnValue != null)
                    {
                        result = await asyncActionWithReturnValue.Invoke();
                    }
                    if (asyncActionWithoutReturnValue != null)
                    {
                        await asyncActionWithoutReturnValue.Invoke();
                        result = new object();
                    }
                    returnValue.SetResult(result);
                }
                catch (Exception ex)
                {
                    executionTask.SetResult(returnValue.SetError(ex));
                }
                finally
                {

                }

                executionTask.SetResult(returnValue);
            }

            public Task<CommandDispatcherOperationAsyncContext> GetAsyncOperation()
            {
                return executionTask.Task;
            }
        }
    }


    public class CommandDispatcherOperationAsyncContext
    {
        public object? Result { get; private set; }

        public CommandDispatcherOperationAsyncContext SetResult(object result)
        {
            if (Result == null)
            {
                Result = result;
                return this;
            }

            throw new InvalidOperationException("Result can only be set once");
        }

        public Exception? Error { get; private set; }

        public CommandDispatcherOperationAsyncContext SetError(Exception error)
        {
            if (Error == null)
            {
                Error = error;
                return this;
            }

            throw new InvalidOperationException("Error can only be set once");
        }

        public bool? TimedOut { get; private set; }

        public CommandDispatcherOperationAsyncContext SetTimedOut(bool timeout)
        {
            if (!TimedOut.HasValue)
            {
                TimedOut = timeout;
                return this;
            }

            throw new InvalidOperationException("TimedOut can only be set once");
        }
    }
}
