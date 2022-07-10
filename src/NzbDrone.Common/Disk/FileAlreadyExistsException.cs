using System;

namespace NzbDrone.Common.Disk
{
    public class FileAlreadyExistsException : Exception
    {
        public string FilePath { get; set; }

        public FileAlreadyExistsException(string message, string filepath) : base(message)
        {
            FilePath = filepath;
        }
    }
}
