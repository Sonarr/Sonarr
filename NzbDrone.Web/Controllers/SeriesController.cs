using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using MvcMiniProfiler;
using NzbDrone.Core;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    [HandleError]
    public class SeriesController : Controller
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly TvDbProvider _tvDbProvider;
        private readonly JobProvider _jobProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        //
        // GET: /Series/

        public SeriesController(SeriesProvider seriesProvider,
                                EpisodeProvider episodeProvider,
                                QualityProvider qualityProvider,
                                TvDbProvider tvDbProvider,
                                JobProvider jobProvider,
                                MediaFileProvider mediaFileProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _qualityProvider = qualityProvider;
            _tvDbProvider = tvDbProvider;
            _jobProvider = jobProvider;
            _mediaFileProvider = mediaFileProvider;
        }

        public ActionResult Index()
        {
            var profiles = _qualityProvider.All();
            ViewData["SelectList"] = new SelectList(profiles, "QualityProfileId", "Name");

            return View();
        }

        [GridAction]
        public ActionResult _AjaxSeriesGrid()
        {
            var series = GetSeriesModels(_seriesProvider.GetAllSeriesWithEpisodeCount()).OrderBy(o => SortHelper.SkipArticles(o.Title));
            return View(new GridModel(series));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveAjaxSeriesEditing(int id, string path, bool monitored, bool seasonFolder, int qualityProfileId)
        {
            var oldSeries = _seriesProvider.GetSeries(id);
            oldSeries.Monitored = monitored;
            oldSeries.SeasonFolder = seasonFolder;
            oldSeries.QualityProfileId = qualityProfileId;
            oldSeries.Path = path;

            _seriesProvider.UpdateSeries(oldSeries);

            var series = GetSeriesModels(_seriesProvider.GetAllSeriesWithEpisodeCount()).OrderBy(o => SortHelper.SkipArticles(o.Title));
            return View(new GridModel(series));
        }

        [GridAction]
        public ActionResult _DeleteAjaxSeriesEditing(int id)
        {
            //Grab the series from the DB so we can remove it from the list we return to the client
            var seriesInDb = _seriesProvider.GetAllSeriesWithEpisodeCount().ToList();

            //Remove this so we don't send it back to the client (since it hasn't really been deleted yet)
            seriesInDb.RemoveAll(s => s.SeriesId == id);

            //Start removing this series
            _jobProvider.QueueJob(typeof(DeleteSeriesJob), id);

            var series = GetSeriesModels(seriesInDb).OrderBy(o => SortHelper.SkipArticles(o.Title));
            return View(new GridModel(series));
        }

        [GridAction]
        public ActionResult _AjaxSeasonGrid(int seriesId, int seasonNumber)
        {
            using (MiniProfiler.StepStatic("Controller"))
            {
                var episodes = GetEpisodeModels(_episodeProvider.GetEpisodesBySeason(seriesId, seasonNumber));
                return View(new GridModel(episodes));
            }
        }

        public JsonResult LocalSearch(string term)
        {
            //Get Results from the local DB and return

            var results = _seriesProvider.SearchForSeries(term).Select(s => new SeriesSearchResultModel
                                                                   {
                                                                       Id = s.SeriesId,
                                                                       Title = s.Title
                                                                   }).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveSeasonIgnore(int seriesId, int seasonNumber, bool ignored)
        {
            _episodeProvider.SetSeasonIgnore(seriesId, seasonNumber, ignored);

            return new JsonResult { Data = "ok" };
        }

        [HttpPost]
        public JsonResult SaveEpisodeIgnore(int episodeId, bool ignored)
        {
            _episodeProvider.SetEpisodeIgnore(episodeId, ignored);

            return new JsonResult { Data = "ok" };
        }

        public ActionResult Details(int seriesId)
        {
            var series = _seriesProvider.GetSeries(seriesId);

            var model = new SeriesModel();

            if (series.AirsDayOfWeek != null)
            {
                model.AirsDayOfWeek = series.AirsDayOfWeek.Value.ToString();
            }
            else
            {
                model.AirsDayOfWeek = "N/A";
            }
            model.Overview = series.Overview;
            model.Seasons = _episodeProvider.GetSeasons(seriesId);
            model.Title = series.Title;
            model.SeriesId = series.SeriesId;
            model.HasBanner = !String.IsNullOrEmpty(series.BannerUrl);

            return View(model);
        }

        private List<SeriesModel> GetSeriesModels(IList<Series> seriesInDb)
        {
            var series = seriesInDb.Select(s => new SeriesModel
                                                    {
                                                        SeriesId = s.SeriesId,
                                                        Title = s.Title,
                                                        AirsDayOfWeek = s.AirsDayOfWeek.ToString(),
                                                        Monitored = s.Monitored,
                                                        Overview = s.Overview,
                                                        Path = s.Path,
                                                        QualityProfileId = s.QualityProfileId,
                                                        QualityProfileName = s.QualityProfile.Name,
                                                        SeasonFolder = s.SeasonFolder,
                                                        Status = s.Status,
                                                        SeasonsCount = s.SeasonCount,
                                                        EpisodeCount = s.EpisodeCount,
                                                        EpisodeFileCount = s.EpisodeFileCount,
                                                        NextAiring = s.NextAiring == null ? String.Empty : s.NextAiring.Value.ToBestDateString()
                                                    }).ToList();

            return series;
        }

        private List<EpisodeModel> GetEpisodeModels(IList<Episode> episodesInDb)
        {
            var episodes = new List<EpisodeModel>();

            foreach (var e in episodesInDb)
            {
                var episodeFileId = 0;
                var episodePath = String.Empty;
                var episodeQuality = String.Empty;

                if (e.EpisodeFile != null)
                {
                    episodePath = e.EpisodeFile.Path;
                    episodeFileId = e.EpisodeFile.EpisodeFileId;
                    episodeQuality = e.EpisodeFile.Quality.ToString();
                }

                var airDate = String.Empty;

                if (e.AirDate != null)
                    airDate = e.AirDate.Value.ToBestDateString();

                episodes.Add(new EpisodeModel
                                 {
                                     EpisodeId = e.EpisodeId,
                                     EpisodeNumber = e.EpisodeNumber,
                                     SeasonNumber = e.SeasonNumber,
                                     Title = e.Title,
                                     Overview = e.Overview,
                                     AirDate = airDate,
                                     Path = episodePath,
                                     EpisodeFileId = episodeFileId,
                                     Status = e.Status.ToString(),
                                     Quality = episodeQuality,
                                     Ignored = e.Ignored
                                 });
            }

            return episodes;
        }
    }
}