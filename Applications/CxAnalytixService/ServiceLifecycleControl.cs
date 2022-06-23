using CxAnalytix.Executive;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;


namespace CxAnalytixService
{
    class ServiceLifecycleControl : ServiceBase
    {

        private CancellationTokenSource _cancelToken = null;
        private Task _serviceTask = null;

        public ServiceLifecycleControl()
        {
            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
        }

        private void stopService()
        {
            if (_cancelToken != null)
            {
                _cancelToken.Cancel();

                if (_serviceTask != null && !_serviceTask.IsCompleted)
                {
                    try
                    {
                        _serviceTask.Wait();
                    }
                    catch (AggregateException)
                    {
                    }
                }
            }
        }

        protected override void OnShutdown()
        {
            stopService();
            base.OnShutdown();
        }


        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            _cancelToken = new CancellationTokenSource();


			_serviceTask = Task.Run( () =>
            {
                ExecuteLoop.Execute(_cancelToken);

            }, _cancelToken.Token);

        }

		protected override void OnStop()
        {
            stopService();
            base.OnStop();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _serviceTask.Dispose();
                _serviceTask = null;
            }
        }
    }
}
