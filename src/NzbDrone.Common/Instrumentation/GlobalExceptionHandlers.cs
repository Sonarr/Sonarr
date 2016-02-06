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

            Console.WriteLine("Task Error: {0}", exception);
            Logger.ErrorException("Task Error: " + exception.Message, exception);
        }

        private static void HandleAppDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null) return;

            if (exception is NullReferenceException &&
                exception.ToString().Contains("Microsoft.AspNet.SignalR.Transports.TransportHeartbeat.ProcessServerCommand"))
            {
                Logger.Warn("SignalR Heartbeat interupted");
                return;
            }

            if (OsInfo.IsMonoRuntime)
            {
                if (exception is TypeInitializationException && exception.InnerException is DllNotFoundException ||
                    exception is DllNotFoundException)
                {
                    Logger.DebugException("Minor Fail: " + exception.Message, exception);
                    return;
                }
            }

            Console.WriteLine("EPIC FAIL: {0}", exception);
            Logger.FatalException("EPIC FAIL: " + exception.Message, exception);
        }
    }
}