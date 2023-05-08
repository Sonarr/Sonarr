using System;
using System.Linq;
using Sentry;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    public static class SentryCleanser
    {
        public static SentryEvent CleanseEvent(SentryEvent sentryEvent)
        {
            try
            {
                if (sentryEvent.Message is not null)
                {
                    sentryEvent.Message.Formatted = CleanseLogMessage.Cleanse(sentryEvent.Message.Formatted);
                    sentryEvent.Message.Message = CleanseLogMessage.Cleanse(sentryEvent.Message.Message);
                    sentryEvent.Message.Params = sentryEvent.Message.Params?.Select(x => CleanseLogMessage.Cleanse(x switch
                    {
                        string str => str,
                        _ => x.ToString()
                    })).ToList();
                }

                if (sentryEvent.Fingerprint.Any())
                {
                    var fingerprint = sentryEvent.Fingerprint.Select(x => CleanseLogMessage.Cleanse(x)).ToList();
                    sentryEvent.SetFingerprint(fingerprint);
                }

                if (sentryEvent.Extra.Any())
                {
                    var extras = sentryEvent.Extra.ToDictionary(x => x.Key, y => (object)CleanseLogMessage.Cleanse(y.Value as string));
                    sentryEvent.SetExtras(extras);
                }

                if (sentryEvent.SentryExceptions is not null)
                {
                    foreach (var exception in sentryEvent.SentryExceptions)
                    {
                        exception.Value = CleanseLogMessage.Cleanse(exception.Value);
                        if (exception.Stacktrace is not null)
                        {
                            foreach (var frame in exception.Stacktrace.Frames)
                            {
                                frame.FileName = ShortenPath(frame.FileName);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return sentryEvent;
        }

        public static Breadcrumb CleanseBreadcrumb(Breadcrumb b)
        {
            try
            {
                var message = CleanseLogMessage.Cleanse(b.Message);
                var data = b.Data?.ToDictionary(x => x.Key, y => CleanseLogMessage.Cleanse(y.Value));
                return new Breadcrumb(message, b.Type, data, b.Category, b.Level);
            }
            catch (Exception)
            {
            }

            return b;
        }

        private static string ShortenPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            // the paths in the stacktrace depend on where it was compiled,
            // not the current OS
            var rootDirs = new[] { "\\src\\", "/src/" };
            foreach (var rootDir in rootDirs)
            {
                var index = path.IndexOf(rootDir, StringComparison.Ordinal);

                if (index > 0)
                {
                    return path.Substring(index + rootDir.Length - 1);
                }
            }

            return path;
        }
    }
}
