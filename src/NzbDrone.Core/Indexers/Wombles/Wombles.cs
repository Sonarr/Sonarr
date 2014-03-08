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

        public override bool SupportsPaging
        {
            get
            {
                return false;
            }
        }

        public override bool SupportsSearching
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
                return new WomblesParser();
            }
        }

        public override IEnumerable<string> RecentFeed
        {
            get { yield return "http://newshost.co.za/rss/?sec=TV&fr=false"; }
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

        public override IEnumerable<string> GetSearchUrls(string query, int offset)
        {
            return new List<string>();
        }
    }
}