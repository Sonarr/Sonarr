using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NLog.Common;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using Sentry;
using Sentry.Protocol;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    [Target("Sentry")]
    public class SentryTarget : TargetWithLayout
    {
        // don't report uninformative SQLite exceptions
        // busy/locked are benign https://forums.sonarr.tv/t/owin-sqlite-error-5-database-is-locked/5423/11
        // The others will be user configuration problems and silt up Sentry
        private static readonly HashSet<SQLiteErrorCode> FilteredSQLiteErrors = new HashSet<SQLiteErrorCode>
        {
            SQLiteErrorCode.Busy,
            SQLiteErrorCode.Locked,
            SQLiteErrorCode.Perm,
            SQLiteErrorCode.ReadOnly,
            SQLiteErrorCode.IoErr,
            SQLiteErrorCode.Corrupt,
            SQLiteErrorCode.Full,
            SQLiteErrorCode.CantOpen,
            SQLiteErrorCode.Auth
        };

        // use string and not Type so we don't need a reference to the project
        // where these are defined
        private static readonly HashSet<string> FilteredExceptionTypeNames = new HashSet<string>
        {
            // UnauthorizedAccessExceptions will just be user configuration issues
            "UnauthorizedAccessException",
            // Filter out people stuck in boot loops
            "CorruptDatabaseException",
            // This also filters some people in boot loops
            "TinyIoCResolutionException"
        };

        public static readonly List<string> FilteredExceptionMessages = new List<string>
        {
            // Swallow the many, many exceptions flowing through from Jackett
            "Jackett.Common.IndexerException",

            // Fix openflixr being stupid with permissions
            "openflixr"
        };

        // exception types in this list will additionally have the exception message added to the
        // sentry fingerprint.  Make sure that this message doesn't vary by exception
        // (e.g. containing a path or a url) so that the sentry grouping is sensible
        private static readonly HashSet<string> IncludeExceptionMessageTypes = new HashSet<string>
        {
            "SQLiteException"
        };

        private static readonly IDictionary<LogLevel, SentryLevel> LoggingLevelMap = new Dictionary<LogLevel, SentryLevel>
        {
            { LogLevel.Debug, SentryLevel.Debug },
            { LogLevel.Error, SentryLevel.Error },
            { LogLevel.Fatal, SentryLevel.Fatal },
            { LogLevel.Info, SentryLevel.Info },
            { LogLevel.Trace, SentryLevel.Debug },
            { LogLevel.Warn, SentryLevel.Warning },
        };

        private static readonly IDictionary<LogLevel, BreadcrumbLevel> BreadcrumbLevelMap = new Dictionary<LogLevel, BreadcrumbLevel>
        {
            { LogLevel.Debug, BreadcrumbLevel.Debug },
            { LogLevel.Error, BreadcrumbLevel.Error },
            { LogLevel.Fatal, BreadcrumbLevel.Critical },
            { LogLevel.Info, BreadcrumbLevel.Info },
            { LogLevel.Trace, BreadcrumbLevel.Debug },
            { LogLevel.Warn, BreadcrumbLevel.Warning },
        };

        private readonly DateTime _startTime = DateTime.UtcNow;
        private readonly IDisposable _sdk;
        private readonly SentryDebounce _debounce;

        private bool _disposed;
        private bool _unauthorized;

        public bool FilterEvents { get; set; }
        public bool SentryEnabled { get; set; }

        public SentryTarget(string dsn)
        {
            _sdk = SentrySdk.Init(o =>
                                  {
                                      o.Dsn = new Dsn(dsn);
                                      o.AttachStacktrace = true;
                                      o.MaxBreadcrumbs = 200;
                                      o.SendDefaultPii = false;
                                      o.Debug = false;
                                      o.DiagnosticsLevel = SentryLevel.Debug;
                                      o.Release = BuildInfo.Release;
                                      o.BeforeSend = x => SentryCleanser.CleanseEvent(x);
                                      o.BeforeBreadcrumb = x => SentryCleanser.CleanseBreadcrumb(x);
                                      o.Environment = BuildInfo.Branch;
                                  });

            InitializeScope();

            _debounce = new SentryDebounce();

            // initialize to true and reconfigure later
            // Otherwise it will default to false and any errors occuring
            // before config file gets read will not be filtered
            FilterEvents = true;

            SentryEnabled = true;
        }

        public void InitializeScope()
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new User
                {
                    Id = HashUtil.AnonymousToken()
                };

                scope.Contexts.App.Name = BuildInfo.AppName;
                scope.Contexts.App.Version = BuildInfo.Version.ToString();
                scope.Contexts.App.StartTime = _startTime;
                scope.Contexts.App.Hash = HashUtil.AnonymousToken();
                scope.Contexts.App.Build = BuildInfo.Release; // Git commit cache?

                scope.SetTag("culture", Thread.CurrentThread.CurrentCulture.Name);
                scope.SetTag("branch", BuildInfo.Branch);
            });
        }

        public void UpdateScope(IOsInfo osInfo)
        {
            SentrySdk.ConfigureScope(scope =>
            {
            });
        }

        public void UpdateScope(Version databaseVersion, int migration, string updateBranch, IPlatformInfo platformInfo)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.Environment = updateBranch;
                scope.SetTag("runtime_version", $"{PlatformInfo.PlatformName} {platformInfo.Version}");

                if (databaseVersion != default(Version))
                {
                    scope.SetTag("sqlite_version", $"{databaseVersion}");
                    scope.SetTag("database_migration", $"{migration}");
                }
            });
        }

        private void OnError(Exception ex)
        {
            if (ex is WebException webException)
            {
                var response = webException.Response as HttpWebResponse;
                var statusCode = response?.StatusCode;
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    _unauthorized = true;
                    _debounce.Clear();
                }
            }

            InternalLogger.Error(ex, "Unable to send error to Sentry");
        }

        private static List<string> GetFingerPrint(LogEventInfo logEvent)
        {
            if (logEvent.Properties.ContainsKey("Sentry"))
            {
                return ((string[])logEvent.Properties["Sentry"]).ToList();
            }

            var fingerPrint = new List<string>
            {
                logEvent.Level.ToString(),
                logEvent.LoggerName,
                logEvent.Message
            };

            var ex = logEvent.Exception;

            if (ex != null)
            {
                fingerPrint.Add(ex.GetType().FullName);
                if (ex.TargetSite != null)
                {
                    fingerPrint.Add(ex.TargetSite.ToString());
                }
                if (ex.InnerException != null)
                {
                    fingerPrint.Add(ex.InnerException.GetType().FullName);
                }
                else if (IncludeExceptionMessageTypes.Contains(ex.GetType().Name))
                {
                    fingerPrint.Add(ex?.Message);
                }
            }

            return fingerPrint;
        }

        public bool IsSentryMessage(LogEventInfo logEvent)
        {
            if (logEvent.Properties.ContainsKey("Sentry"))
            {
                return logEvent.Properties["Sentry"] != null;
            }

            if (logEvent.Level >= LogLevel.Error && logEvent.Exception != null)
            {
                if (FilterEvents)
                {
                    var sqlEx = logEvent.Exception as SQLiteException;
                    if (sqlEx != null && FilteredSQLiteErrors.Contains(sqlEx.ResultCode))
                    {
                        return false;
                    }

                    if (FilteredExceptionTypeNames.Contains(logEvent.Exception.GetType().Name))
                    {
                        return false;
                    }

                    if (FilteredExceptionMessages.Any(x => logEvent.Exception.Message.Contains(x)))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }


        protected override void Write(LogEventInfo logEvent)
        {
            if (_unauthorized || !SentryEnabled)
            {
                return;
            }

            try
            {
                SentrySdk.AddBreadcrumb(logEvent.FormattedMessage, logEvent.LoggerName, level: BreadcrumbLevelMap[logEvent.Level]);

                // don't report non-critical events without exceptions
                if (!IsSentryMessage(logEvent))
                {
                    return;
                }

                var fingerPrint = GetFingerPrint(logEvent);
                if (!_debounce.Allowed(fingerPrint))
                {
                    return;
                }

                var extras = logEvent.Properties.ToDictionary(x => x.Key.ToString(), x => (object)x.Value.ToString());
                extras.Remove("Sentry");

                if (logEvent.Exception != null)
                {
                    foreach (DictionaryEntry data in logEvent.Exception.Data)
                    {
                        extras.Add(data.Key.ToString(), data.Value.ToString());
                    }
                }

                var sentryEvent = new SentryEvent(logEvent.Exception)
                {
                    Level = LoggingLevelMap[logEvent.Level],
                    Logger = logEvent.LoggerName,
                    Message = logEvent.FormattedMessage
                };

                sentryEvent.SetExtras(extras);
                sentryEvent.SetFingerprint(fingerPrint);

                SentrySdk.CaptureEvent(sentryEvent);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        // https://stackoverflow.com/questions/2496311/implementing-idisposable-on-a-subclass-when-the-parent-also-implements-idisposab
        protected override void Dispose(bool disposing)
        {
            // Only do something if we're not already disposed
            if (_disposed)
            {
                // If disposing == true, we're being called from a call to base.Dispose().  In this case, we Dispose() our logger
                // If we're being called from a finalizer, our logger will get finalized as well, so no need to do anything.
                if (disposing)
                {
                    _sdk?.Dispose();
                }
                // Flag us as disposed.  This allows us to handle multiple calls to Dispose() as well as ObjectDisposedException
                _disposed = true;
            }

            // This should always be safe to call multiple times!
            // We could include it in the check for disposed above, but I left it out to demonstrate that it's safe
            base.Dispose(disposing);
        }
    }
}
