using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class FileSharingTalk : IndexerBase
    {
        [Inject]
        public FileSharingTalk(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       string.Format("http://filesharingtalk.com/ng_rss.php?uid={0}&ps={1}&category=tv&subcategory=x264sd,x264720,xvid,webdl720,x2641080", 
                                       _configProvider.FileSharingTalkUid, _configProvider.FileSharingTalkSecret)
                                   };
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configProvider.FileSharingTalkUid) &&
                       !string.IsNullOrWhiteSpace(_configProvider.FileSharingTalkSecret);
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
            get { return "File Sharing Talk"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
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