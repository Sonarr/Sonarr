using System;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public sealed class PutioTorrentStatus
    {
      public static readonly String Completed = "COMPLETED";
      public static readonly String Downloading = "DOWNLOADING";
      public static readonly String Error = "ERROR";
      public static readonly String InQueue = "IN_QUEUE";
    }
}
