using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using FluentValidation.Results;
using System.Linq;

namespace NzbDrone.Core.Indexers.IPTorrents
{
    public class IPTorrents : IndexerBase<IPTorrentsSettings>
    {
        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Torrent;
            }
        }

        public override Int32 SupportedPageSize
        {
            get
            {
                return 0;
            }
        }

        public override Boolean SupportsSearch
        {
            get
            {
                return false;
            }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new IPTorrentsRssParser();
            }
        }

        public override IEnumerable<String> RecentFeed
        {
            get
            {
                yield return Settings.Url;
            }
        }

        public override IEnumerable<String> GetEpisodeSearchUrls(List<String> titles, Int32 tvRageId, Int32 seasonNumber, Int32 episodeNumber)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetDailyEpisodeSearchUrls(List<String> titles, Int32 tvRageId, DateTime date)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetAnimeEpisodeSearchUrls(List<String> titles, Int32 tvRageId, Int32 absoluteEpisodeNumber)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetSeasonSearchUrls(List<String> titles, Int32 tvRageId, Int32 seasonNumber, Int32 offset)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetSearchUrls(String query, Int32 offset)
        {
            return Enumerable.Empty<String>();
        }

        public override ValidationResult Test()
        {
            return new ValidationResult();
        }
    }
}