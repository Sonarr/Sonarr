namespace NzbDrone.Core.Download.Clients.LibTorrent.Models
{
    /// <summary> Re-Implements LibTorrent <a href="https://libtorrent.org/reference-Torrent_Status.html#state_t">state_t</a> </summary>
    public enum LibTorrentStatus
    {
        /// <summary> The torrent has not started its download yet, and is currently checking existing files. </summary>
        checking_files = 1,

        /// <summary> The torrent is trying to download metadata from peers. This implies the ut_metadata extension is in use. </summary>
        downloading_metadata = 2,

        /// <summary> The torrent is being downloaded. This is the state most torrents will be in most of the time. The progress meter will tell how much of the files that has been downloaded. </summary>
        downloading = 3,

        /// <summary> In this state the torrent has finished downloading but still doesn't have the entire torrent. i.e. some pieces are filtered and won't get downloaded. </summary>
        finished = 4,

        /// <summary> In this state the torrent has finished downloading and is a pure seeder. </summary>
        seeding = 5,

        /// <summary> If the torrent was started in full allocation mode, this indicates that the (disk) storage for the torrent is allocated. </summary>
        unused_enum_for_backwards_compatibility_allocating = 6,

        /// <summary> The torrent is currently checking the fast resume data and comparing it to the files on disk. This is typically completed in a fraction of a second, but if you add a large number of torrents at once, they will queue up. </summary>
        checking_resume_data = 7
    }
}
