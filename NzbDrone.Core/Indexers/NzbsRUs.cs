using System.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers
{
    public class NzbsRUs : IndexerBase
    {
        public NzbsRUs(HttpProvider httpProvider, IConfigService configService) : base(httpProvider, configService)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                           {
                               string.Format(
                                   "https://www.nzbsrus.com/rssfeed.php?cat=91,75&i={0}&h={1}",
                                   _configService.NzbsrusUId,
                                   _configService.NzbsrusHash)
                           };
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configService.NzbsrusUId) &&
                       !string.IsNullOrWhiteSpace(_configService.NzbsrusHash);
            }
        }

        public override string Name
        {
            get { return "NzbsRUs"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            return new List<string>();
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            return new List<string>();
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            return new List<string>();
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"\d+\.\d{1,2} \w{3}", RegexOptions.IgnoreCase).Value;
                currentResult.Size = Parser.GetReportSize(sizeString);
            }

            return currentResult;
        }
    }
}