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
    public class UpcomingController : Controller
    {
        private UpcomingEpisodesProvider _upcomingEpisodesProvider;

        public UpcomingController(UpcomingEpisodesProvider upcomingEpisodesProvider)
        {
            _upcomingEpisodesProvider = upcomingEpisodesProvider;
        }

        //
        // GET: /Upcoming/

        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult _AjaxBindingYesterday()
        {
            var upcoming = _upcomingEpisodesProvider.Yesterday().Select(e => new UpcomingEpisodeModel
            {
                SeriesId = e.Series.SeriesId,
                SeriesName = e.Series.Title,
                SeasonNumber = e.SeasonNumber,
                EpisodeNumber = e.EpisodeNumber,
                Title = e.Title,
                Overview = e.Overview,
                AirDate = e.AirDate
            });
            
            return View(new GridModel(upcoming));
        }

        [GridAction]
        public ActionResult _AjaxBindingToday()
        {
            var upcoming = _upcomingEpisodesProvider.Today().Select(e => new UpcomingEpisodeModel
            {
                SeriesId =  e.Series.SeriesId,
                SeriesName = e.Series.Title,
                SeasonNumber = e.SeasonNumber,
                EpisodeNumber = e.EpisodeNumber,
                Title = e.Title,
                Overview = e.Overview,
                AirDate = e.AirDate
            });

            return View(new GridModel(upcoming));
        }

        [GridAction]
        public ActionResult _AjaxBindingWeek()
        {
            var upcoming = _upcomingEpisodesProvider.Week().Select(e => new UpcomingEpisodeModel
            {
                SeriesId = e.Series.SeriesId,
                SeriesName = e.Series.Title,
                SeasonNumber = e.SeasonNumber,
                EpisodeNumber = e.EpisodeNumber,
                Title = e.Title,
                Overview = e.Overview,
                AirDate = e.AirDate
            });

            return View(new GridModel(upcoming));
        }
    }
}
