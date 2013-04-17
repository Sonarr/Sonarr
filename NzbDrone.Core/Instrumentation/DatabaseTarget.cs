using System;
using NLog.Config;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NzbDrone.Common;

namespace NzbDrone.Core.Instrumentation
{

    public class DatabaseTarget : TargetWithLayout
    {
        private readonly ILogRepository _repository;

        public DatabaseTarget(ILogRepository repository)
        {
            _repository = repository;
        }

        public void Register()
        {
            Layout = new SimpleLayout("${callsite:className=false:fileName=false:includeSourcePath=false:methodName=true}");

            LogManager.Configuration.AddTarget("DbLogger", this);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, this));
            LogManager.ConfigurationReloaded += (sender, args) => Register();
        }


        protected override void Write(LogEventInfo logEvent)
        {
            var log = new Log();
            log.Time = logEvent.TimeStamp;
            log.Message = logEvent.FormattedMessage;
            log.Method = Layout.Render(logEvent);

            log.Logger = logEvent.LoggerName;

            if (log.Logger.StartsWith("NzbDrone."))
            {
                log.Logger = log.Logger.Remove(0, 9);
            }

            if (logEvent.Exception != null)
            {
                if (String.IsNullOrWhiteSpace(log.Message))
                {
                    log.Message = logEvent.Exception.Message;
                }
                else
                {
                    log.Message += ": " + logEvent.Exception.Message;
                }


                log.Exception = logEvent.Exception.ToString();
                log.ExceptionType = logEvent.Exception.GetType().ToString();
            }


            log.Level = logEvent.Level.Name;

            _repository.Insert(log);
        }
    }
}