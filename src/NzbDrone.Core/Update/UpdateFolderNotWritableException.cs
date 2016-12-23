namespace NzbDrone.Core.Update
{
    public class UpdateFolderNotWritableException : UpdateFailedException
    {
        public UpdateFolderNotWritableException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UpdateFolderNotWritableException(string message)
            : base(message)
        {
        }
    }
}
