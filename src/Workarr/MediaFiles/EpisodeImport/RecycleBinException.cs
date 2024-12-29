using System.Runtime.Serialization;

namespace Workarr.MediaFiles.EpisodeImport
{
    public class RecycleBinException : DirectoryNotFoundException
    {
        public RecycleBinException()
        {
        }

        public RecycleBinException(string message)
            : base(message)
        {
        }

        public RecycleBinException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RecycleBinException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
