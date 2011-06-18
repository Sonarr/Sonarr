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
            var upcomingDb = _upcomingEpisodesProvider.Yesterday();
            var upcoming = new List<UpcomingEpisodeModel>();
                
            foreach (var item in upcomingDb)
            {
                var series = _seriesProvider.GetSeries(item.SeriesId);

                upcoming.Add(new UpcomingEpisodeModel
                                                    {
                                                        SeriesId = series.SeriesId,
                                                        EpisodeId = item.EpisodeId,
                                                        SeriesName = series.Title,
                                                        SeasonNumber = item.SeasonNumber,
                                                        EpisodeNumber = item.EpisodeNumber,
                                                        Title = item.Title,
                                                        Overview = item.Overview,
                                                        AirDate = item.AirDate.Add(Convert.ToDateTime(series.AirTimes).TimeOfDay)
                                                    });
            }

            return View(new GridModel(upcoming));
        }

        [GridAction]
        public ActionResult _AjaxBindingToday()
        {
            var upcomingDb = _upcomingEpisodesProvider.Today();
            var upcoming = new List<UpcomingEpisodeModel>();

            foreach (var item in upcomingDb)
            {
                var series = _seriesProvider.GetSeries(item.SeriesId);

                upcoming.Add(new UpcomingEpisodeModel
                {
                    SeriesId = series.SeriesId,
                    EpisodeId = item.EpisodeId,
                    SeriesName = series.Title,
                    SeasonNumber = item.SeasonNumber,
                    EpisodeNumber = item.EpisodeNumber,
                    Title = item.Title,
                    Overview = item.Overview,
                    AirDate = item.AirDate.Add(Convert.ToDateTime(series.AirTimes).TimeOfDay)
                });
            }

            return View(new GridModel(upcoming));
        }

        [GridAction]
        public ActionResult _AjaxBindingTomorrow()
        {
            var upcomingDb = _upcomingEpisodesProvider.Tomorrow();
            var upcoming = new List<UpcomingEpisodeModel>();

            foreach (var item in upcomingDb)
            {
                var series = _seriesProvider.GetSeries(item.SeriesId);

                upcoming.Add(new UpcomingEpisodeModel
                {
                    SeriesId = series.SeriesId,
                    EpisodeId = item.EpisodeId,
                    SeriesName = series.Title,
                    SeasonNumber = item.SeasonNumber,
                    EpisodeNumber = item.EpisodeNumber,
                    Title = item.Title,
                    Overview = item.Overview,
                    AirDate = item.AirDate.Add(Convert.ToDateTime(series.AirTimes).TimeOfDay)
                });
            }

            return View(new GridModel(upcoming));
        }

        [GridAction]
        public ActionResult _AjaxBindingWeek()
        {
            var upcomingDb = _upcomingEpisodesProvider.Week();
            var upcoming = new List<UpcomingEpisodeModel>();

            foreach (var item in upcomingDb)
            {
                var series = _seriesProvider.GetSeries(item.SeriesId);

                upcoming.Add(new UpcomingEpisodeModel
                {
                    SeriesId = series.SeriesId,
                    EpisodeId = item.EpisodeId,
                    SeriesName = series.Title,
                    SeasonNumber = item.SeasonNumber,
                    EpisodeNumber = item.EpisodeNumber,
                    Title = item.Title,
                    Overview = item.Overview,
                    AirDate = item.AirDate.Add(Convert.ToDateTime(series.AirTimes).TimeOfDay)
                });
            }

            return View(new GridModel(upcoming));
        }
    }
}