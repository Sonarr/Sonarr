using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.FileSharingTalk
{
    public class FileSharingTalk : IndexerWithSetting<FileSharingTalkSetting>
    {
        public FileSharingTalk(IProviderIndexerSetting settingProvider)
            : base(settingProvider)
        {
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                yield return string.Format(
                        "http://filesharingtalk.com/ng_rss.php?uid={0}&ps={1}&category=tv&subcategory=x264sd,x264720,xvid,webdl720,x2641080",
                        Settings.Uid, Settings.Secret);
            }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new FileSharingTalkParser();
            }
        }

        public override string Name
        {
            get { return "FileSharingTalk"; }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            return new List<string>();
        }
    }
}