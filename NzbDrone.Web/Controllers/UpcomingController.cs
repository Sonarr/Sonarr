using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Core;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class UpcomingController : Controller
    {
        private readonly UpcomingEpisodesProvider _upcomingEpisodesProvider;

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
            var upcoming = _upcomingEpisodesProvider.Yesterday().Select(u => new UpcomingEpisodeModel
            {
                SeriesId = u.Series.SeriesId,
                EpisodeId = u.EpisodeId,
                SeriesTitle = u.Series.Title,
                SeasonNumber = u.SeasonNumber,
                EpisodeNumber = u.EpisodeNumber,
                Title = u.Title,
                Overview = u.Overview,
                AirDateTime = u.AirDate.Value.Add(Convert.ToDateTime(u.Series.AirTimes).TimeOfDay),
                AirDate = u.AirDate.Value.ToBestDateString(),
                AirTime = Convert.ToDateTime(u.Series.AirTimes).ToShortTimeString(),
                Status = u.Status.ToString()
            });

            return View(new GridModel(upcoming));
        }

        [GridAction]
        public ActionResult _AjaxBindingToday()
        {
            var upcoming = _upcomingEpisodesProvider.Today().Select(u => new UpcomingEpisodeModel
            {
                SeriesId = u.Series.SeriesId,
                EpisodeId = u.EpisodeId,
                SeriesTitle = u.Series.Title,
                SeasonNumber = u.SeasonNumber,
                EpisodeNumber = u.EpisodeNumber,
                Title = u.Title,
                Overview = u.Overview,
                AirDateTime = u.AirDate.Value.Add(Convert.ToDateTime(u.Series.AirTimes).TimeOfDay),
                AirDate = u.AirDate.Value.ToBestDateString(),
                AirTime = Convert.ToDateTime(u.Series.AirTimes).ToShortTimeString(),
                Status = u.Status.ToString()
            });

            return View(new GridModel(upcoming));
        }

        [GridAction]
        public ActionResult _AjaxBindingTomorrow()
        {
            var upcoming = _upcomingEpisodesProvider.Tomorrow().Select(u => new UpcomingEpisodeModel
            {
                SeriesId = u.Series.SeriesId,
                EpisodeId = u.EpisodeId,
                SeriesTitle = u.Series.Title,
                SeasonNumber = u.SeasonNumber,
                EpisodeNumber = u.EpisodeNumber,
                Title = u.Title,
                Overview = u.Overview,
                AirDateTime = u.AirDate.Value.Add(Convert.ToDateTime(u.Series.AirTimes).TimeOfDay),
                AirDate = u.AirDate.Value.ToBestDateString(),
                AirTime = Convert.ToDateTime(u.Series.AirTimes).ToShortTimeString(),
                Status = u.Status.ToString()
            });

            return View(new GridModel(upcoming));
        }

        [GridAction]
        public ActionResult _AjaxBindingWeek()
        {
            var upcoming = _upcomingEpisodesProvider.Week().Select(u => new UpcomingEpisodeModel
            {
                SeriesId = u.Series.SeriesId,
                EpisodeId = u.EpisodeId,
                SeriesTitle = u.Series.Title,
                SeasonNumber = u.SeasonNumber,
                EpisodeNumber = u.EpisodeNumber,
                Title = u.Title,
                Overview = u.Overview,
                AirDateTime = u.AirDate.Value.Add(Convert.ToDateTime(u.Series.AirTimes).TimeOfDay),
                AirDate = u.AirDate.Value.ToBestDateString(),
                AirTime = Convert.ToDateTime(u.Series.AirTimes).ToShortTimeString(),
                Status = u.Status.ToString()
            });

            return View(new GridModel(upcoming));
        }
    }
}