using System.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers
{
    public class FileSharingTalk : IndexerBase
    {
        public FileSharingTalk(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       string.Format("http://filesharingtalk.com/ng_rss.php?uid={0}&ps={1}&category=tv&subcategory=x264sd,x264720,xvid,webdl720,x2641080", 
                                       _configService.FileSharingTalkUid, _configService.FileSharingTalkSecret)
                                   };
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configService.FileSharingTalkUid) &&
                       !string.IsNullOrWhiteSpace(_configService.FileSharingTalkSecret);
            }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            return new List<string>();
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            return new List<string>();
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            return new List<string>();
        }

        public override string Name
        {
            get { return "FileSharingTalk"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            return item.Id;
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                currentResult.Size = 0;
                currentResult.Age = 0;
            }
            
            return currentResult;
        }
    }
}