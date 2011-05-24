using System;
using System.Collections.Generic;
using System.IO;

namespace Migrator.Framework.Loggers
{
    public class SqlScriptFileLogger : ILogger, IDisposable
    {
        private readonly ILogger _innerLogger;
        private TextWriter _streamWriter;

        public SqlScriptFileLogger(ILogger logger, TextWriter streamWriter)
        {
            _innerLogger = logger;
            _streamWriter = streamWriter;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Dispose();
                _streamWriter = null;
            }
        }

        #endregion

        public void Log(string format, params object[] args)
        {
            _innerLogger.Log(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _innerLogger.Warn(format, args);
        }

        public void Trace(string format, params object[] args)
        {
            _innerLogger.Trace(format, args);
        }

        public void ApplyingDBChange(string sql)
        {
            _innerLogger.ApplyingDBChange(sql);
            _streamWriter.WriteLine(sql);
        }

        public void Started(List<long> appliedVersions, long finalVersion)
        {
            _innerLogger.Started(appliedVersions, finalVersion);
        }

        public void MigrateUp(long version, string migrationName)
        {
            _innerLogger.MigrateUp(version, migrationName);
        }

        public void MigrateDown(long version, string migrationName)
        {
            _innerLogger.MigrateDown(version, migrationName);
        }

        public void Skipping(long version)
        {
            _innerLogger.Skipping(version);
        }

        public void RollingBack(long originalVersion)
        {
            _innerLogger.RollingBack(originalVersion);
        }

        public void Exception(long version, string migrationName, Exception ex)
        {
            _innerLogger.Exception(version, migrationName, ex);
        }

        public void Exception(string message, Exception ex)
        {
            _innerLogger.Exception(message, ex);
        }

        public void Finished(List<long> appliedVersions, long currentVersion)
        {
            _innerLogger.Finished(appliedVersions, currentVersion);
            _streamWriter.Close();
        }
    }
}
