using CxAnalytix.Exceptions;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Executive
{
    public class ExecuteLoop : ExecuteOnce
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ExecuteLoop));

        public static new void Execute(CancellationTokenSource t)
        {

            do
            {

                try
                {
                    ExecuteOnce.Execute(t);
                }
                catch (ProcessFatalException pfe)
                {
                    Fatal(pfe, t);
                    break;
                }
                catch (TypeInitializationException ex)
                {
                    Fatal(ex, t);
                    break;
                }
                catch (Exception ex)
                {
                    _log.Error("Vulnerability data transformation aborted due to unhandled exception.", ex);
                }


                GC.Collect();

                using (var delay = Task.Delay(Service.ProcessPeriodMinutes * 60 * 1000, t.Token))
                    delay.Wait(t.Token);

            } while (!t.Token.IsCancellationRequested);

            _log.Info("Execution complete, ending.");

        }
        private static void Fatal(Exception ex, CancellationTokenSource ct)
        {
            _log.Error("Fatal exception caught, program ending.", ex);
            ct.Cancel();
            Process.GetCurrentProcess().Kill(true);

        }


    }
}
