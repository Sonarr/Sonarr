using System;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using NzbDrone.Core.Download.Extensions;

namespace NzbDrone.Core.Download.Clients.RTorrent
{
    public class RTorrentTorrent
    {
        public RTorrentTorrent()
        {
        }

        public RTorrentTorrent(XElement element)
        {
            var data = element.Descendants("value").ToList();

            Name = data[0].GetStringValue();
            Hash = data[1].GetStringValue();
            Path = data[2].GetStringValue();
            Category = HttpUtility.UrlDecode(data[3].GetStringValue());
            TotalSize = data[4].GetLongValue();
            RemainingSize = data[5].GetLongValue();
            DownRate = data[6].GetLongValue();
            Ratio = data[7].GetLongValue();
            IsOpen = Convert.ToBoolean(data[8].GetLongValue());
            IsActive = Convert.ToBoolean(data[9].GetLongValue());
            IsFinished = Convert.ToBoolean(data[10].GetLongValue());
            FinishedTime = data[11].GetLongValue();
        }

        public string Name { get; set; }
        public string Hash { get; set; }
        public string Path { get; set; }
        public string Category { get; set; }
        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public long DownRate { get; set; }
        public long Ratio { get; set; }
        public long FinishedTime { get; set; }
        public bool IsFinished { get; set; }
        public bool IsOpen { get; set; }
        public bool IsActive { get; set; }
    }
}
