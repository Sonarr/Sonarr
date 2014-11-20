using System;

namespace MonoTorrent
{
    [Serializable]
    public class TorrentException : Exception
    {
        public TorrentException()
            : base()
        {
        }

        public TorrentException(string message)
            : base(message)
        {
        }

        public TorrentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TorrentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
