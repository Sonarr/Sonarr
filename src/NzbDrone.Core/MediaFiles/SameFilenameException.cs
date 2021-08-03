using System;

namespace NzbDrone.Core.MediaFiles
{
    public class SameFilenameException : Exception
    {
        public string Filename { get; set; }

        public SameFilenameException(string message, string filename)
            : base(message)
        {
            Filename = filename;
        }
    }
}
