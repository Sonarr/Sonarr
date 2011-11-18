using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Search;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public class Newznab : IndexerBase
    {
        private readonly NewznabProvider _newznabProvider;

        [Inject]
        public Newznab(HttpProvider httpProvider, ConfigProvider configProvider, NewznabProvider newznabProvider)
            : base(httpProvider, configProvider)
          {
              _newznabProvider = newznabProvider;
          }

        protected override string[] Urls
        {
            get { return GetUrls(); }
        }

        public override string Name
        {
            get { return "Newznab"; }
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
                    searchUrls.Add(String.Format("{0}&limit=100&q={1}&season{2}&ep{3}", url,
                                                 searchModel.SeriesTitle, searchModel.SeasonNumber, searchModel.EpisodeNumber));
                }

                if (searchModel.SearchType == SearchType.SeasonSearch)
                {
                    //Todo: Allow full season searching to process individual episodes
                    //searchUrls.Add(String.Format("{0}&limit=100&q={1}&season{2}", url, searchModel.SeriesTitle, searchModel.SeasonNumber));
                    searchUrls.Add(String.Format("{0}&limit=100&q={1}+Season", url, searchModel.SeriesTitle));
                }
            }

            return searchUrls;
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @">\d+\.\d{1,2} \w{2}</a>", RegexOptions.IgnoreCase).Value;

                currentResult.Size = Parser.GetReportSize(sizeString);
            }
            return currentResult;
        }

        private string[] GetUrls()
        {
            var urls = new List<string>();
            var newznzbIndexers = _newznabProvider.Enabled();

            foreach(var newznabDefinition in newznzbIndexers)
            {
                if (!String.IsNullOrWhiteSpace(newznabDefinition.ApiKey))
                    urls.Add(String.Format("{0}/api?t=tvsearch&cat=5030,5040&apikey={1}", newznabDefinition.Url,
                                        newznabDefinition.ApiKey));

                else
                    urls.Add(String.Format("{0}/api?t=tvsearch&cat=5030,5040", newznabDefinition.Url));
            }

            return urls.ToArray();
        }
    }
}