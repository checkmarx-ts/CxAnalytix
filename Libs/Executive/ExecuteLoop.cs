using CxAnalytix.Exceptions;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Executive
{
    public class ExecuteLoop : ExecuteOnce
    {
        private static readonly ILog appLog = LogManager.GetLogger(typeof(ExecuteLoop));

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
                    appLog.Error("Vulnerability data transformation aborted due to unhandled exception.", ex);
                }


                Task.Delay(Service.ProcessPeriodMinutes * 60 * 1000, t.Token).Wait();
            } while (!t.Token.IsCancellationRequested);

        }
        private static void Fatal(Exception ex, CancellationTokenSource ct)
        {
            appLog.Error("Fatal exception caught, program ending.", ex);
            ct.Cancel();
        }


    }
}
