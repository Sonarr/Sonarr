namespace Workarr.Disk
{
    public class FileAlreadyExistsException : Exception
    {
        public string Filename { get; set; }

        public FileAlreadyExistsException(string message, string filename)
            : base(message)
        {
            Filename = filename;
        }
    }
}
