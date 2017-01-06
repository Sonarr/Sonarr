using System;
using System.Collections.Generic;
using SharpRaven.Data;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    public class SonarrJsonPacketFactory : IJsonPacketFactory
    {
        private static string ShortenPath(string path)
        {

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var index = path.IndexOf("\\src\\", StringComparison.Ordinal);

            if (index <= 0)
            {
                return path;
            }

            return path.Substring(index + "\\src".Length);
        }

        public JsonPacket Create(string project, SentryEvent @event)
        {
            var packet = new SonarrSentryPacket(project, @event);

            try
            {
                foreach (var exception in packet.Exceptions)
                {
                    foreach (var frame in exception.Stacktrace.Frames)
                    {
                        frame.Filename = ShortenPath(frame.Filename);
                    }
                }
            }
            catch (Exception)
            {

            }

            return packet;
        }


        [Obsolete]
        public JsonPacket Create(string project, SentryMessage message, ErrorLevel level = ErrorLevel.Info, IDictionary<string, string> tags = null,
            string[] fingerprint = null, object extra = null)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public JsonPacket Create(string project, Exception exception, SentryMessage message = null, ErrorLevel level = ErrorLevel.Error,
            IDictionary<string, string> tags = null, string[] fingerprint = null, object extra = null)
        {
            throw new NotImplementedException();
        }
    }
}