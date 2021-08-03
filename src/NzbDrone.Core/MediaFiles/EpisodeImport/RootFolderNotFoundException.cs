using System;
using System.IO;
using System.Runtime.Serialization;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public class RootFolderNotFoundException : DirectoryNotFoundException
    {
        public RootFolderNotFoundException()
        {
        }

        public RootFolderNotFoundException(string message)
            : base(message)
        {
        }

        public RootFolderNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RootFolderNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
