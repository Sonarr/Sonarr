using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.History
{
    public class History : ModelBase
    {
        public const string DOWNLOAD_CLIENT = "downloadClient";

        public History()
        {
            Data = new Dictionary<string, string>();
        }

        public int EpisodeId { get; set; }
        public int SeriesId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public Episode Episode { get; set; }
        public Series Series { get; set; }
        public HistoryEventType EventType { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public Language Language { get; set; }

        public string DownloadId { get; set; }

    }

    public enum HistoryEventType
    {
        Unknown = 0,
        Grabbed = 1,
        SeriesFolderImported = 2,
        DownloadFolderImported = 3,
        DownloadFailed = 4,
        EpisodeFileDeleted = 5,
        EpisodeFileRenamed = 6
    }
}