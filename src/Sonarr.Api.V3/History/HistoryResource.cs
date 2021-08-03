using System;
using System.Collections.Generic;
using NzbDrone.Core.History;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using Sonarr.Api.V3.Episodes;
using Sonarr.Api.V3.Series;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.History
{
    public class HistoryResource : RestResource
    {
        public int EpisodeId { get; set; }
        public int SeriesId { get; set; }
        public string SourceTitle { get; set; }
        public Language Language { get; set; }
        public QualityModel Quality { get; set; }
        public bool QualityCutoffNotMet { get; set; }
        public bool LanguageCutoffNotMet { get; set; }
        public DateTime Date { get; set; }
        public string DownloadId { get; set; }

        public EpisodeHistoryEventType EventType { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public EpisodeResource Episode { get; set; }
        public SeriesResource Series { get; set; }
    }

    public static class HistoryResourceMapper
    {
        public static HistoryResource ToResource(this EpisodeHistory model)
        {
            if (model == null)
            {
                return null;
            }

            return new HistoryResource
            {
                Id = model.Id,

                EpisodeId = model.EpisodeId,
                SeriesId = model.SeriesId,
                SourceTitle = model.SourceTitle,
                Language = model.Language,
                Quality = model.Quality,

                //QualityCutoffNotMet
                Date = model.Date,
                DownloadId = model.DownloadId,

                EventType = model.EventType,

                Data = model.Data

                //Episode
                //Series
            };
        }
    }
}
