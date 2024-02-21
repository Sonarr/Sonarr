using System.Collections.Generic;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public sealed class PorlaTorrentDetail
    {
        public long active_duration { get; set; }
        public long all_time_download { get; set; }
        public long all_time_upload { get; set; }
        public string category { get; set; }
        public long download_rate { get; set; }
        public string error { get; set; } //this should maybe definitly be string
        public long eta { get; set; }
        public long finished_duration { get; set; }
        public List<string> flags { get; set; }
        public PorlaTorrent info_hash { get; set; }
        public long last_download { get; set; }
        public long last_upload { get; set; }
        public long list_peers { get; set; }
        public long list_seeds { get; set; }
        public Dictionary<string, string> metadata { get; set; }
        public bool moving_storage { get; set; }
        public string name { get; set; }
        public long num_peers { get; set; }
        public long num_seeds { get; set; }
        public double progress { get; set; }
        public long queue_position { get; set; }
        public double ratio { get; set; }
        public string save_path { get; set; }
        public long seeding_duration { get; set; }
        public string session { get; set; }
        public long size { get; set; }
        public long state { get; set; }
        public List<string> tags { get; set; }
        public long total { get; set; }
        public long total_done { get; set; }
        public long upload_rate { get; set; }
    }

    public sealed class ResponsePorlaTorrentList
    {
        public string order_by { get; set; }
        public string order_by_dir { get; set; }
        public long page { get; set; }
        public long page_size { get; set; }
        public List<PorlaTorrentDetail> torrents { get; set; }
        public long torrents_total { get; set; }
        public long torrents_total_unfiltered { get; set; }
    }
}
