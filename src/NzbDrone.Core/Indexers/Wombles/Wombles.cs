using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class Wombles : IndexerBase<NullConfig>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Usenet; } }
        public override bool SupportsSearch { get { return false; } }

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

        public override IEnumerable<string> GetEpisodeSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int offset)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(List<String> titles, int tvRageId, DateTime date)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetAnimeEpisodeSearchUrls(List<String> titles, int tvRageId, int absoluteEpisodeNumber)
        {
            return new string[0];
        }

        public override IEnumerable<string> GetSearchUrls(string query, int offset)
        {
            return new List<string>();
        }

        public override ValidationResult Test()
        {
            return new ValidationResult();
        }
    }
}