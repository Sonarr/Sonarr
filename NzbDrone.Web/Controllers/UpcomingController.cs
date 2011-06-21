using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class UpcomingController : Controller
    {
        private readonly UpcomingEpisodesProvider _upcomingEpisodesProvider;
        private readonly SeriesProvider _seriesProvider;

        public UpcomingController(UpcomingEpisodesProvider upcomingEpisodesProvider, SeriesProvider seriesProvider)
        {
            _upcomingEpisodesProvider = upcomingEpisodesProvider;
            _seriesProvider = seriesProvider;
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
                SeriesName = u.Series.Title,
                SeasonNumber = u.SeasonNumber,
                EpisodeNumber = u.EpisodeNumber,
                Title = u.Title,
                Overview = u.Overview,
                AirDate = u.AirDate.Add(Convert.ToDateTime(u.Series.AirTimes).TimeOfDay)
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
                SeriesName = u.Series.Title,
                SeasonNumber = u.SeasonNumber,
                EpisodeNumber = u.EpisodeNumber,
                Title = u.Title,
                Overview = u.Overview,
                AirDate = u.AirDate.Add(Convert.ToDateTime(u.Series.AirTimes).TimeOfDay)
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
                SeriesName = u.Series.Title,
                SeasonNumber = u.SeasonNumber,
                EpisodeNumber = u.EpisodeNumber,
                Title = u.Title,
                Overview = u.Overview,
                AirDate = u.AirDate.Add(Convert.ToDateTime(u.Series.AirTimes).TimeOfDay)
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
                SeriesName = u.Series.Title,
                SeasonNumber = u.SeasonNumber,
                EpisodeNumber = u.EpisodeNumber,
                Title = u.Title,
                Overview = u.Overview,
                AirDate = u.AirDate.Add(Convert.ToDateTime(u.Series.AirTimes).TimeOfDay)
            });

            return View(new GridModel(upcoming));
        }
    }
}