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

        public static Task<T> DisposeTask<T>(this Task<T> task)
        {
            if (task != null)
            {
                task.Wait(WAIT_MAX);
                if (task.IsCompleted)
                    task.Dispose();
            }

            return null;
        }
    }
}
