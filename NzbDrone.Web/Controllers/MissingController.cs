using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using NzbDrone.Core;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Web.Models;
using ServiceStack.Text;

namespace NzbDrone.Web.Controllers
{
    public class MissingController : Controller
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly ConfigProvider _configProvider;

        public MissingController(EpisodeProvider episodeProvider, ConfigProvider configProvider)
        {
            _episodeProvider = episodeProvider;
            _configProvider = configProvider;
        }

        public ActionResult Index()
        {
            var missingEpisodes = _episodeProvider.EpisodesWithoutFiles(false);
            var ignoreArticles = _configProvider.IgnoreArticlesWhenSortingSeries;

            var missing = missingEpisodes.Select(e => new MissingEpisodeModel
            {
                EpisodeId = e.EpisodeId,
                SeriesId = e.SeriesId,
                EpisodeNumbering = string.Format("{0}x{1:00}", e.SeasonNumber, e.EpisodeNumber),
                EpisodeTitle = e.Title,
                Overview = e.Overview,
                SeriesTitle = e.Series.Title,
                SeriesTitleSorter = ignoreArticles ? e.Series.Title.IgnoreArticles() : e.Series.Title,
                AirDateSorter = e.AirDate.Value.ToString("o", CultureInfo.InvariantCulture),
                AirDate = e.AirDate.Value.ToBestDateString()
            });

            JsConfig.IncludeNullValues = true;
            return View(missing);
        }
    }
}
