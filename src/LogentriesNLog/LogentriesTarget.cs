using System;
using LogentriesCore;
using LogentriesNLog.fastJSON;
using NLog;
using NLog.Targets;

namespace LogentriesNLog
{
    [Target("Logentries")]
    public sealed class LogentriesTarget : TargetWithLayout
    {
        private AsyncLogger logentriesAsync;

        public LogentriesTarget()
        {
            logentriesAsync = new AsyncLogger();
            logentriesAsync.setImmediateFlush(true);
        }


        /** Debug flag. */
        public bool Debug
        {
            get { return logentriesAsync.getDebug(); }
            set { logentriesAsync.setDebug(value); }
        }

        /** Is using DataHub parameter flag. - ste to true if it is needed to send messages to DataHub instance. */
        public bool IsUsingDataHub
        {
            get { return logentriesAsync.getIsUsingDataHab(); }
            set { logentriesAsync.setIsUsingDataHub(value); }
        }

        /** DataHub server address */
        public String DataHubAddr
        {
            get { return logentriesAsync.getDataHubAddr(); }
            set { logentriesAsync.setDataHubAddr(value); }
        }

        /** DataHub server port */
        public int DataHubPort
        {
            get { return logentriesAsync.getDataHubPort(); }
            set { logentriesAsync.setDataHubPort(value); }
        }

        /** Option to set Token programmatically or in Appender Definition */
        public string Token
        {
            get { return logentriesAsync.getToken(); }
            set { logentriesAsync.setToken(value); }
        }

        /** HTTP PUT Flag */
        public bool HttpPut
        {
            get { return logentriesAsync.getUseHttpPut(); }
            set { logentriesAsync.setUseHttpPut(value); }
        }

        /** SSL/TLS parameter flag */
        public bool Ssl
        {
            get { return logentriesAsync.getUseSsl(); }
            set { logentriesAsync.setUseSsl(value); }
        }

        /** ACCOUNT_KEY parameter for HTTP PUT logging */
        public String Key
        {
            get { return logentriesAsync.getAccountKey(); }
            set { logentriesAsync.setAccountKey(value); }
        }

        /** LOCATION parameter for HTTP PUT logging */
        public String Location
        {
            get { return logentriesAsync.getLocation(); }
            set { logentriesAsync.setLocation(value); }
        }

        /* LogHostname - switch that defines whether add host name to the log message */
        public bool LogHostname
        {
            get { return logentriesAsync.getUseHostName(); }
            set { logentriesAsync.setUseHostName(value); }
        }

        /* HostName - user-defined host name. If empty the library will try to obtain it automatically */
        public String HostName
        {
            get { return logentriesAsync.getHostName(); }
            set { logentriesAsync.setHostName(value); }
        }

        /* User-defined log message ID */
        public String LogID
        {
            get { return logentriesAsync.getLogID(); }
            set { logentriesAsync.setLogID(value); }
        }

        public bool KeepConnection { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            //Render message content
            var log = new Log
            {
                Level = logEvent.Level.ToString(),
                Logger = logEvent.LoggerName,
                Message = logEvent.FormattedMessage,
                Time = logEvent.TimeStamp,
            };

            //NLog can pass null references of Exception
            if (logEvent.Exception != null)
            {
                log.Exception = logEvent.Exception.ToString();
                log.ExceptionType = logEvent.Exception.GetType().ToString();
            }

            logentriesAsync.AddLine(JSON.Instance.ToJSON(log));
        }

        protected override void CloseTarget()
        {
            base.CloseTarget();

            logentriesAsync.interruptWorker();
        }
    }

    public class Log
    {
        public string Message { get; set; }

        public DateTime Time { get; set; }

        public string Logger { get; set; }

        public string Exception { get; set; }

        public string ExceptionType { get; set; }

        public String Level { get; set; }
    }
}
