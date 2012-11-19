using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository.Search;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class SearchHistoryController : Controller
    {
        private readonly SearchHistoryProvider _searchHistoryProvider;

        public SearchHistoryController(SearchHistoryProvider searchHistoryProvider)
        {
            _searchHistoryProvider = searchHistoryProvider;
        }

        public ActionResult Index()
        {
            var results = _searchHistoryProvider.AllSearchHistory();

            var model = results.Select(s => new SearchHistoryModel
            {
                Id = s.Id,
                SearchTime = s.SearchTime.ToString(),
                DisplayName = GetDisplayName(s),
                ReportCount = s.TotalItems,
                Successful = s.SuccessfulCount > 0
            });

            return View(model);
        }

        public ActionResult Details(int searchId)
        {
            var searchResult = _searchHistoryProvider.GetSearchHistory(searchId);
            var model = new SearchDetailsModel
            {
                Id = searchResult.Id,
                DisplayName = GetDisplayName(searchResult),
                SearchHistoryItems =
                searchResult.SearchHistoryItems.Select(s => new SearchItemModel
                {
                    Id = s.Id,
                    ReportTitle = s.ReportTitle,
                    Indexer = s.Indexer,
                    NzbUrl = s.NzbUrl,
                    NzbInfoUrl = s.NzbInfoUrl,
                    Success = s.Success,
                    SearchError = s.SearchError.AddSpacesToEnum().Replace("None", "Grabbed"),
                    Quality = s.Quality.ToString(),
                    QualityInt = (int)s.Quality,
                    Proper = s.Proper,
                    Age = s.Age,
                    Size = s.Size.ToBestFileSize(1),
                    Language = s.Language.ToString()
                }).ToList()
            };

            return View(model);
        }

        public JsonResult ForceDownload(int id)
        {
            _searchHistoryProvider.ForceDownload(id);

            return JsonNotificationResult.Info("Success", "Requested episode has been sent to download client");
        }

        public string GetDisplayName(SearchHistory searchResult)
        {
            if (!searchResult.EpisodeNumber.HasValue)
            {
                return String.Format("{0} - Season {1}", searchResult.SeriesTitle, searchResult.SeasonNumber);
            }

            string episodeString;

            if (searchResult.IsDaily)
                episodeString = searchResult.AirDate.ToShortDateString().Replace('/', '-');

            else
                episodeString = String.Format("S{0:00}E{1:00}", searchResult.SeasonNumber,
                                              searchResult.EpisodeNumber);

            return String.Format("{0} - {1} - {2}", searchResult.SeriesTitle, episodeString, searchResult.EpisodeTitle);
        }
    }
}
