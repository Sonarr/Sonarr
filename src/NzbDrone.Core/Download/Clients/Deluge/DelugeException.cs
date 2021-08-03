namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeException : DownloadClientException
    {
        public int Code { get; set; }

        public DelugeException(string message, int code)
            : base(message + " (code " + code + ")")
        {
            Code = code;
        }
    }
}
