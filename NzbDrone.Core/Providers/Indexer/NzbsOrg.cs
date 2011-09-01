using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using Ninject;
using NzbDrone.Core.Model.Search;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbsOrg : IndexerBase
    {
          [Inject]
        public NzbsOrg(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       string.Format("http://nzbs.org/rss.php?type=1&i={0}&h={1}&num=50&dl=1", _configProvider.NzbsOrgUId, _configProvider.NzbsOrgHash)
                                   };
            }
        }

        public override string Name
        {
            get { return "Nzbs.org"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id;
        }

        protected override IList<string> GetSearchUrls(SearchModel searchModel)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                if (searchModel.SearchType == SearchType.EpisodeSearch)
                {
                    searchUrls.Add(String.Format("{0}&action=search&q={1}+s{2:00}e{3:00}", url,
                                                 searchModel.SeriesTitle, searchModel.SeasonNumber, searchModel.EpisodeNumber));
                }

                if (searchModel.SearchType == SearchType.PartialSeasonSearch)
                {
                    searchUrls.Add(String.Format("{0}&action=search&q={1}+S{2:00}E{3}",
                        url, searchModel.SeriesTitle, searchModel.SeasonNumber, searchModel.EpisodePrefix));
                }

                if (searchModel.SearchType == SearchType.SeasonSearch)
                {
                    searchUrls.Add(String.Format("{0}&action=search&q={1}+Season", url, searchModel.SeriesTitle));
                    searchUrls.Add(String.Format("{0}&action=search&q={1}+S{2:00}", url, searchModel.SeriesTitle, searchModel.SeasonNumber));
                }
            }

            return searchUrls;
        }

    }
}