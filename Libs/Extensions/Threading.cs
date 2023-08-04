using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Extensions
{
    public static class Threading
    {
        private static readonly int WAIT_MAX = 120000;

        private static void doDispose(Task task)
        {
            if (task != null)
            {
                task.Wait(WAIT_MAX);
                if (task.IsCompleted)
                    task.Dispose();
            }
        }

        public static Task<T> DisposeTask<T>(this Task<T> task)
        {
            doDispose(task);
            return null;
        }

        public static Task DisposeTask(this Task task)
        {
            doDispose(task);
            return null;
        }
        public static void DisposeTasks(this IEnumerable<Task> tasks)
        {
            foreach(var task in tasks)
                doDispose(task);
        }

        public static void SafeWaitToEnd(this Task task)
        {
            if (task == null)
                return;

            try
            {
                if (!task.IsCompleted)
                    task.Wait();
            }
            // Eat the exceptions so it ensures all the tasks end.
            catch (AggregateException)
            {
            }
            catch (TaskCanceledException)
            {
            }
        }


        public static void SafeWaitAllToEnd(this IEnumerable<Task> tasks)
        {
            if (tasks == null)
                return;

            foreach (var task in tasks)
            {
                if (task == null)
                    continue;

                try
                {
                    if (!task.IsCompleted)
                        task.Wait();
                }
                // Eat the exceptions so it ensures all the tasks end.
                catch (AggregateException)
                {
                }
                catch (TaskCanceledException)
                {
                }
            }

        }

    }
}
