using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class MissingController : Controller
    {
        private readonly EpisodeProvider _episodeProvider;

        public MissingController(EpisodeProvider episodeProvider)
        {
            _episodeProvider = episodeProvider;
        }

        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult _AjaxBinding()
        {
            var missingEpisodes = _episodeProvider.EpisodesWithoutFiles(false);

            var missing = missingEpisodes.Select(e => new MissingEpisodeModel
            {
                EpisodeId = e.EpisodeId,
                SeasonNumber = e.SeasonNumber,
                EpisodeNumber = e.EpisodeNumber,
                EpisodeTitle = e.Title,
                Overview = e.Overview,
                SeriesTitle = e.Series.Title,
                AirDate = e.AirDate.Value,
            });

            return View(new GridModel(missing));
        }
    }
}
