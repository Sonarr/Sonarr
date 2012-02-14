using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using NzbDrone.Core;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;

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
            var missingEpisodes = _episodeProvider.EpisodesWithoutFiles(false);

            var missing = missingEpisodes.Select(e => new MissingEpisodeModel
            {
                EpisodeId = e.EpisodeId,
                SeriesId = e.SeriesId,
                EpisodeNumbering = string.Format("{0}x{1:00}", e.SeasonNumber, e.EpisodeNumber),
                EpisodeTitle = e.Title,
                Overview = e.Overview,
                SeriesTitle = e.Series.Title,
                AirDate = e.AirDate.Value.ToString("MM/dd/yyyy"),
                AirDateString = e.AirDate.Value.ToBestDateString()
            });

            var serialized = new JavaScriptSerializer().Serialize(missing);

            return View((object)serialized);
        }
    }
}
