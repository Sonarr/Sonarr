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
            //TODO: possible subsonic bug, IQuarible causes some issues so ToList() is called
            //https://github.com/subsonic/SubSonic-3.0/issues/263

            var missing = _episodeProvider.EpisodesWithoutFiles(true).Select(e => new MissingEpisodeModel
            {
                EpisodeId = e.EpisodeId,
                SeasonNumber = e.SeasonNumber,
                EpisodeNumber = e.EpisodeNumber,
                EpisodeTitle = e.Title,
                Overview = e.Overview,
                SeriesTitle = e.Series.Title,
                AirDate = e.AirDate,
            });

            return View(new GridModel(missing));
        }
    }
}
