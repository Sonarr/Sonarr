using System;
using System.Linq;
using Sentry;
using Sentry.Protocol;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    public static class SentryCleanser
    {
        public static SentryEvent CleanseEvent(SentryEvent sentryEvent)
        {
            try
            {
                sentryEvent.Message = CleanseLogMessage.Cleanse(sentryEvent.Message);

                if (sentryEvent.Fingerprint != null)
                {
                    var fingerprint = sentryEvent.Fingerprint.Select(x => CleanseLogMessage.Cleanse(x)).ToList();
                    sentryEvent.SetFingerprint(fingerprint);
                }

                if (sentryEvent.Extra != null)
                {
                    var extras = sentryEvent.Extra.ToDictionary(x => x.Key, y => (object)CleanseLogMessage.Cleanse((string)y.Value));
                    sentryEvent.SetExtras(extras);
                }

                foreach (var exception in sentryEvent.SentryExceptions)
                {
                    exception.Value = CleanseLogMessage.Cleanse(exception.Value);
                    foreach (var frame in exception.Stacktrace.Frames)
                    {
                        frame.FileName = ShortenPath(frame.FileName);
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
