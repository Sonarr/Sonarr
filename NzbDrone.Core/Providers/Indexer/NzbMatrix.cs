using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using Ninject;
using NzbDrone.Core.Model.Search;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbMatrix : IndexerBase
    {
          [Inject]
        public NzbMatrix(HttpProvider httpProvider, ConfigProvider configProvider) : base(httpProvider, configProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                           {
                               string.Format(
                                   "http://rss.nzbmatrix.com/rss.php?page=download&username={0}&apikey={1}&subcat=6,41&english=1&scenename=1&num=50",
                                   _configProvider.NzbMatrixUsername,
                                   _configProvider.NzbMatrixApiKey)
                           };
            }
        }

        public override string Name
        {
            get { return "NzbMatrix"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override IList<string> GetSearchUrls(SearchModel searchModel)
        {
            var searchUrls = new List<String>();

            foreach (var url in Urls)
            {
                if (searchModel.SearchType == SearchType.EpisodeSearch)
                {
                    searchUrls.Add(String.Format("{0}&term={1}+s{2:00}e{3:00}", url, searchModel.SeriesTitle,
                                                 searchModel.SeasonNumber, searchModel.EpisodeNumber));
                }

                if (searchModel.SearchType == SearchType.PartialSeasonSearch)
                {
                    searchUrls.Add(String.Format("{0}&term={1}+S{2:00}E{3}",
                        url, searchModel.SeriesTitle, searchModel.SeasonNumber, searchModel.EpisodePrefix));
                }

                if (searchModel.SearchType == SearchType.SeasonSearch)
                {
                    searchUrls.Add(String.Format("{0}&term={1}+Season", url, searchModel.SeriesTitle));
                    searchUrls.Add(String.Format("{0}&term={1}+S{2:00}", url, searchModel.SeriesTitle, searchModel.SeasonNumber));
                }
            }

            return searchUrls;
        }

    }
}