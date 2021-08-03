using System.Data;
using System.Data.SQLite;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Instrumentation
{
    public class DatabaseTarget : TargetWithLayout, IHandle<ApplicationShutdownRequested>
    {
        private const string INSERT_COMMAND = "INSERT INTO [Logs]([Message],[Time],[Logger],[Exception],[ExceptionType],[Level]) " +
                                 "VALUES(@Message,@Time,@Logger,@Exception,@ExceptionType,@Level)";

        private readonly SQLiteConnection _connection;

        public DatabaseTarget(IConnectionStringFactory connectionStringFactory)
        {
            _connection = new SQLiteConnection(connectionStringFactory.LogDbConnectionString);
            _connection.Open();
        }

        public void Register()
        {
            var target = new SlowRunningAsyncTargetWrapper(this) { TimeToSleepBetweenBatches = 500 };

            Rule = new LoggingRule("*", LogLevel.Info, target);

            LogManager.Configuration.AddTarget("DbLogger", target);
            LogManager.Configuration.LoggingRules.Add(Rule);
            LogManager.ConfigurationReloaded += OnLogManagerOnConfigurationReloaded;
            LogManager.ReconfigExistingLoggers();
        }

        public void UnRegister()
        {
            LogManager.ConfigurationReloaded -= OnLogManagerOnConfigurationReloaded;
            LogManager.Configuration.RemoveTarget("DbLogger");
            LogManager.Configuration.LoggingRules.Remove(Rule);
            LogManager.ReconfigExistingLoggers();
            Dispose();
        }

        private void OnLogManagerOnConfigurationReloaded(object sender, LoggingConfigurationReloadedEventArgs args)
        {
            Register();
        }

        public LoggingRule Rule { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                var log = new Log();
                log.Time = logEvent.TimeStamp;
                log.Message = CleanseLogMessage.Cleanse(logEvent.FormattedMessage);

                log.Logger = logEvent.LoggerName;

                if (log.Logger.StartsWith("NzbDrone."))
                {
                    log.Logger = log.Logger.Remove(0, 9);
                }

                if (logEvent.Exception != null)
                {
                    if (string.IsNullOrWhiteSpace(log.Message))
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

                var sqlCommand = new SQLiteCommand(INSERT_COMMAND, _connection);

                sqlCommand.Parameters.Add(new SQLiteParameter("Message", DbType.String) { Value = log.Message });
                sqlCommand.Parameters.Add(new SQLiteParameter("Time", DbType.DateTime) { Value = log.Time.ToUniversalTime() });
                sqlCommand.Parameters.Add(new SQLiteParameter("Logger", DbType.String) { Value = log.Logger });
                sqlCommand.Parameters.Add(new SQLiteParameter("Exception", DbType.String) { Value = log.Exception });
                sqlCommand.Parameters.Add(new SQLiteParameter("ExceptionType", DbType.String) { Value = log.ExceptionType });
                sqlCommand.Parameters.Add(new SQLiteParameter("Level", DbType.String) { Value = log.Level });

                sqlCommand.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                InternalLogger.Error(ex, "Unable to save log event to database");
                throw;
            }
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            if (LogManager.Configuration != null && LogManager.Configuration.LoggingRules.Contains(Rule))
            {
                UnRegister();
            }
        }
    }
}
