using System;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using Sonarr.Api.V3.Series;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Blacklist
{
    public class BlacklistResource : RestResource
    {
        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public string SourceTitle { get; set; }
        public Language Language { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }

        public SeriesResource Series { get; set; }
    }

    public static class BlacklistResourceMapper
    {
        public static BlacklistResource MapToResource(this NzbDrone.Core.Blacklisting.Blacklist model)
        {
            if (model == null) return null;

            return new BlacklistResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                EpisodeIds = model.EpisodeIds,
                SourceTitle = model.SourceTitle,
                Language = model.Language,
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
