using System.Collections.Generic;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using Logger = Loggly.Logger;

namespace NzbDrone.Common.Instrumentation
{
    public class LogglyTarget : TargetWithLayout
    {
        private Logger _logger;

        public LogglyTarget()
        {
            Layout = new SimpleLayout("${callsite:className=false:fileName=false:includeSourcePath=false:methodName=true}");
            
        }

        protected override void InitializeTarget()
        {
            string apiKey = string.Empty;

            if (RuntimeInfo.IsProduction)
            {
                apiKey = "4c4ecb69-d1b9-4e2a-b54b-b0c4cc143a95";
            }
            else
            {
                apiKey = "d344a321-b107-45c4-a548-77138f446510";
            }

            _logger = new Logger(apiKey);
        }


        protected override void Write(LogEventInfo logEvent)
        {
            var dictionary = new Dictionary<string, object>();

            if (logEvent.Exception != null)
            {
                dictionary.Add("ex", logEvent.Exception.ToString());
                dictionary.Add("extyp", logEvent.Exception.GetType().Name);
                dictionary.Add("hash", logEvent.GetHash());

                foreach (var key in logEvent.Exception.Data.Keys)
                {
                    dictionary.Add(key.ToString(), logEvent.Exception.Data[key]);
                }
            }

            dictionary.Add("logger", logEvent.LoggerName);
            dictionary.Add("method", Layout.Render(logEvent));
            dictionary.Add("level", logEvent.Level.Name);
            dictionary.Add("message", logEvent.GetFormattedMessage());
            dictionary.Add("ver", BuildInfo.Version.ToString());

            _logger.Log(dictionary.ToJson());
        }
    }
}