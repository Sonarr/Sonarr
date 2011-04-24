using System;
using NLog;
using NLog.Targets;
using SubSonic.Repository;

namespace NzbDrone.Core.Instrumentation
{
    public class SubsonicTarget : Target
    {
        private readonly IRepository _repository;

        public SubsonicTarget(IRepository repository)
        {
            _repository = repository;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var log = new Log();
            log.Time = logEvent.TimeStamp;
            log.Message = logEvent.FormattedMessage;

            if (logEvent.UserStackFrame != null)
            {
                log.Method = logEvent.UserStackFrame.GetMethod().Name;
            }



            log.Logger = logEvent.LoggerName;

            if (logEvent.Exception != null)
            {

                log.Message += ": " + logEvent.Exception.Message;

                log.Exception = logEvent.Exception.ToString();
                log.ExceptionType = logEvent.Exception.GetType().ToString();
            }


            log.Level = logEvent.Level.Name;


            _repository.Add(log);
        }
    }
}