using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LibSharpHelp
{
    public class TaskWrappedAction
    {
        public Task current { get; private set; }
        readonly Action work;
        public TaskWrappedAction(Action work)
        {
            this.work = work;
        }
        [DebuggerStepThrough]
        public void Work()
        {
            TaskCompletionSource<int> ti = new TaskCompletionSource<int>();
            current = ti.Task;
            try
            {
                work();
                ti.SetResult(0);
            }
#if !DEBUG
            catch (Exception e)
            {
                ti.SetException(e);
                throw;
            }
#endif
            finally { }
        }
    }
    public static class TaskHelpers
    {
        public static Task ContinueAfter(this Task current, Func<Task> after)
        {
            TaskCompletionSource<bool> nextComplete = new TaskCompletionSource<bool>();
            current.ContinueWith(async c =>
            {
                await after();
                nextComplete.SetResult(true);
            });
            return nextComplete.Task;
        }
        public static Task ContinueAfter<A>(this Task<A> current, Func<Task> after)
        {
            TaskCompletionSource<bool> nextComplete = new TaskCompletionSource<bool>();
            current.ContinueWith(async c =>
            {
                await after();
                nextComplete.SetResult(true);
            });
            return nextComplete.Task;
        }
        public static Task<B> ContinueAfter<B>(this Task current, Func<Task<B>> after)
        {
            TaskCompletionSource<B> nextComplete = new TaskCompletionSource<B>();
            current.ContinueWith(async c => nextComplete.SetResult(await after()));
            return nextComplete.Task;
        }
        public static Task<B> ContinueAfter<A,B>(this Task<A> current, Func<Task<B>> after)
        {
            TaskCompletionSource<B> nextComplete = new TaskCompletionSource<B>();
            current.ContinueWith(async c => nextComplete.SetResult(await after()));
            return nextComplete.Task;
        }
    }
}
