using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;
using RestSharp;

namespace NzbDrone.Core.Indexers.Primewire
{
    public enum PrimewireSearchCategory
    {
        Movies = 1,
        Tv = 2
    }
    public class PrimewireRequestGenerator : IIndexerRequestGenerator
    {
        private readonly IParsingService parsingService;
        private readonly IEpisodeRepository episodeRepository;
        public PrimewireSettings Settings { get; set; }

        public PrimewireRequestGenerator(IParsingService parsingService, IEpisodeRepository episodeRepository)
        {
            this.parsingService = parsingService;
            this.episodeRepository = episodeRepository;
        }

        public IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(searchCriteria.Series, PrepareQuery(queryTitle), PrimewireSearchCategory.Tv, searchCriteria.Episodes, searchCriteria.SeasonNumber));
            }
            return pageableRequests;
        }


        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(searchCriteria.Series, PrepareQuery(queryTitle), PrimewireSearchCategory.Tv, searchCriteria.Episodes, searchCriteria.SeasonNumber));
            }
            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(searchCriteria.Series, PrepareQuery(queryTitle), PrimewireSearchCategory.Tv, searchCriteria.Episodes));
            }
            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(searchCriteria.Series, PrepareQuery(queryTitle), PrimewireSearchCategory.Tv, searchCriteria.Episodes));
            }
            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(searchCriteria.Series, PrepareQuery(queryTitle), PrimewireSearchCategory.Tv, searchCriteria.Episodes));
            }
            return pageableRequests;
        }

        private readonly Regex seriesPattern = new Regex("<a href=\"/watch-(?<id>\\d{0,10})-.*?<h2>(?<title>.*?)</h2>", RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        private IEnumerable<IndexerRequest> GetPagedRequests(Series series, string title, PrimewireSearchCategory category, List<Episode> episodes, int? season = null)
        {
            var client = new RestClient(Settings.BaseUrl);
            var request = new RestRequest("index.php")
                .AddParameter("search_keywords", title)
                .AddParameter("search_section", (int) category);

            var response = client.Execute(request);

            var entries = seriesPattern.Matches(response.Content).Cast<Match>()
                                       .Select(a => new
                                       {
                                           Id = Convert.ToInt32(a.Groups["id"].Value),
                                           Title = a.Groups["title"].Value,
                                           Series = parsingService.GetSeries(a.Groups["title"].Value)
                                       });

            var entry = entries.FirstOrDefault(a => a.Series != null && a.Series.Id == series.Id);
            var requests = new List<IndexerRequest>();
            if (entry == null) return requests;


            var seasons = season.HasValue ? new[] {season.Value} : series.Seasons.Select(a => a.SeasonNumber).ToArray();
            foreach (int seasonNumb in seasons)
            {
                var episodeNumbs = episodes.Where(a=>a.AirDateUtc < DateTime.UtcNow).Select(a => a.EpisodeNumber).ToArray();
                foreach (int episodeNumb in episodeNumbs)
                {
                    requests.Add(new IndexerRequest(string.Format("{0}/tv-{1}-X/season-{2}-episode-{3}", Settings.BaseUrl.TrimEnd("/"), entry.Id, seasonNumb, episodeNumb), HttpAccept.Html));
                }
             }



            return requests;
        }
        
        //readonly Regex episodesRegex = new Regex("<div class=\"tv_episode_item\"> <a href=\"/tv-(?<id>\\d{1,10})-.*?season-(?<season>\\d{1,3})-episode-(?<episode>\\d{1,3})\">", RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
        //    foreach (var match in episodesRegex.Matches(content).Cast<Match>())
        //    {
        //        int id = Convert.ToInt32(match.Groups["id"]);
        //        int season = Convert.ToInt32(match.Groups["season"]);
        //        int episode = Convert.ToInt32(match.Groups["episode"]);
        //    }

        private String PrepareQuery(String query)
        {
            return query.Replace('+', ' ');
        }
    }
}