using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Core;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Web.Models;

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
            var upcomingEpisodes = GetUpcomingEpisodeModels(_upcomingEpisodesProvider.RecentEpisodes());


            var upcoming = new UpcomingEpisodesModel
                               {
                                   Yesterday = upcomingEpisodes.Where(e => e.AirDateTime.Date == DateTime.Today.AddDays(-1)).ToList(),
                                   Today = upcomingEpisodes.Where(e => e.AirDateTime.Date == DateTime.Today).ToList(),
                                   Tomorrow = upcomingEpisodes.Where(e => e.AirDateTime.Date == DateTime.Today.AddDays(1)).ToList(),
                                   TwoDays = upcomingEpisodes.Where(e => e.AirDateTime.Date == DateTime.Today.AddDays(2)).ToList(),
                                   ThreeDays = upcomingEpisodes.Where(e => e.AirDateTime.Date == DateTime.Today.AddDays(3)).ToList(),
                                   FourDays = upcomingEpisodes.Where(e => e.AirDateTime.Date == DateTime.Today.AddDays(4)).ToList(),
                                   FiveDays = upcomingEpisodes.Where(e => e.AirDateTime.Date == DateTime.Today.AddDays(5)).ToList(),
                                   SixDays = upcomingEpisodes.Where(e => e.AirDateTime.Date == DateTime.Today.AddDays(6)).ToList(),
                                   Later = upcomingEpisodes.Where(e => e.AirDateTime.Date >= DateTime.Today.AddDays(7)).ToList()
                               };
            
            return View(upcoming);
        }

        private List<UpcomingEpisodeModel> GetUpcomingEpisodeModels(List<Episode> episodes)
        {
            return episodes.Select(u => new UpcomingEpisodeModel
            {
                SeriesId = u.Series.SeriesId,
                EpisodeId = u.EpisodeId,
                SeriesTitle = u.Series.Title,
                EpisodeNumbering = String.Format("{0}x{1:00}", u.SeasonNumber, u.EpisodeNumber),
                Title = u.Title,
                Overview = u.Overview,
                AirDateTime = GetDateTime(u.AirDate.Value, u.Series.AirTimes),
                AirDate = u.AirDate.Value.ToBestDateString(),
                AirTime = String.IsNullOrEmpty(u.Series.AirTimes) ? "?" : Convert.ToDateTime(u.Series.AirTimes).ToShortTimeString(),
                Status = u.Status.ToString()
            }).OrderBy(e => e.AirDateTime).ToList();
        }

        private DateTime GetDateTime(DateTime airDate, string airTime)
        {
            if (String.IsNullOrWhiteSpace(airTime))
                return airDate;

            return airDate.Add(Convert.ToDateTime(airTime).TimeOfDay);
        }
    }
}