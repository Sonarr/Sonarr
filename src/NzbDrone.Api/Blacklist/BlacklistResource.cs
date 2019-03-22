using System;
using System.Collections.Generic;
using Sonarr.Http.REST;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Series;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;

namespace NzbDrone.Api.Blacklist
{
    public class BlacklistResource : RestResource
    {
        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }
        public Language Language { get; set; }

        public SeriesResource Series { get; set; }
    }

    public static class BlacklistResourceMapper
    {
        public static BlacklistResource MapToResource(this Core.Blacklisting.Blacklist model)
        {
            if (model == null) return null;

            return new BlacklistResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                EpisodeIds = model.EpisodeIds,
                SourceTitle = model.SourceTitle,
                Quality = model.Quality,
                Date = model.Date,
                Protocol = model.Protocol,
                Indexer = model.Indexer,
                Message = model.Message,

                Series = model.Series.ToResource()
            };
        }
    }
}
