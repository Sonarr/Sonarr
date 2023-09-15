namespace NzbDrone.Core.Download.Clients.Putio
{
    public sealed class PutioTorrentStatus
    {
      public static readonly string Completed = "COMPLETED";
      public static readonly string Downloading = "DOWNLOADING";
      public static readonly string Error = "ERROR";
      public static readonly string InQueue = "IN_QUEUE";
    }
}
