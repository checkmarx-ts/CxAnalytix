using log4net;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "CxAnalyticsExportService.log4net", Watch = true)]

namespace CxAnalyticsExportService
{
    class ServiceLifecycleControl : ServiceBase
    {

        private ILog _log = LogManager.GetLogger (typeof (ServiceLifecycleControl));


        public ServiceLifecycleControl ()
        {
            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
            CanHandlePowerEvent = true;
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
