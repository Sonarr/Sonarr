using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public class EpisodeHistory : ModelBase
    {
        public const string DOWNLOAD_CLIENT = "downloadClient";
        public const string SERIES_MATCH_TYPE = "seriesMatchType";

        public EpisodeHistory()
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
        public EpisodeHistoryEventType EventType { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public Language Language { get; set; }
        public string DownloadId { get; set; }
    }

    public enum EpisodeHistoryEventType
    {
        Unknown = 0,
        Grabbed = 1,
        SeriesFolderImported = 2,
        DownloadFolderImported = 3,
        DownloadFailed = 4,
        EpisodeFileDeleted = 5,
        EpisodeFileRenamed = 6,
        DownloadIgnored = 7
    }
}
