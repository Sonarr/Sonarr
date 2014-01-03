using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class Wombles : IndexerBase<NullConfig>
    {
        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new WomblesParser();
            }
        }

        public override IEnumerable<string> RecentFeed
        {
            get { yield return "http://nzb.isasecret.com/rss/?sec=TV&fr=false"; }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int offset)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetSearchUrls(string query, int offset, int limit)
        {
            return new List<string>();
        }
    }
}