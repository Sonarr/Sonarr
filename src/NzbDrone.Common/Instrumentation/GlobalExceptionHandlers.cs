using System;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Instrumentation
{
    public static class GlobalExceptionHandlers
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(GlobalExceptionHandlers));
        public static void Register()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleAppDomainException;
            TaskScheduler.UnobservedTaskException += HandleTaskException;
        }

        private static void HandleTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var exception = e.Exception;

            if (exception.InnerException is ObjectDisposedException disposedException && disposedException.ObjectName == "System.Net.HttpListenerRequest")
            {
                // We don't care about web connections
                return;
            }

            Console.WriteLine("Task Error: {0}", exception);
            Logger.Error(exception, "Task Error");
        }

        private static void HandleAppDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null)
            {
                return;
            }

            if (exception is NullReferenceException &&
                exception.ToString().Contains("Microsoft.AspNet.SignalR.Transports.TransportHeartbeat.ProcessServerCommand"))
            {
                Logger.Warn("SignalR Heartbeat interrupted");
                return;
            }

            Console.WriteLine("EPIC FAIL: {0}", exception);
            Logger.Fatal(exception, "EPIC FAIL.");
        }
    }
}
