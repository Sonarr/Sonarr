using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using MvcMiniProfiler;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
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

        public ActionResult RssSync()
        {
            _jobProvider.QueueJob(typeof(RssSyncJob));
            return RedirectToAction("Index");
        }

        public ActionResult SeasonEditor(int seriesId)
        {
            var model = new List<SeasonEditModel>();

            var seasons = _episodeProvider.GetSeasons(seriesId);

            foreach (var season in seasons)
            {
                var seasonEdit = new SeasonEditModel();
                seasonEdit.Monitored = !_episodeProvider.IsIgnored(seriesId, season);
                seasonEdit.SeasonNumber = season;
                seasonEdit.SeriesId = seriesId;
                seasonEdit.SeasonString = GetSeasonString(season);

                model.Add(seasonEdit);
            }

            return View(model);
        }

        public ActionResult GetSingleSeasonView(SeasonEditModel model)
        {
            return PartialView("SingleSeason", model);
        }

        [GridAction]
        public ActionResult _AjaxSeriesGrid()
        {
            var series = GetSeriesModels(_seriesProvider.GetAllSeriesWithEpisodeCount());
            return View(new GridModel(series));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveAjaxSeriesEditing(int id, string path, bool monitored, bool seasonFolder, int qualityProfileId, List<SeasonEditModel> seasonEditor)
        {
            var oldSeries = _seriesProvider.GetSeries(id);
            oldSeries.Monitored = monitored;
            oldSeries.SeasonFolder = seasonFolder;
            oldSeries.QualityProfileId = qualityProfileId;

            _seriesProvider.UpdateSeries(oldSeries);

            var series = GetSeriesModels(_seriesProvider.GetAllSeriesWithEpisodeCount());
            return View(new GridModel(series));
        }

        [GridAction]
        public ActionResult _DeleteAjaxSeriesEditing(int id)
        {
            //Grab the series from the DB so we can remove it from the list we return to the client
            var seriesInDb = _seriesProvider.GetAllSeries().ToList();

            //Remove this so we don't send it back to the client (since it hasn't really been deleted yet)
            seriesInDb.RemoveAll(s => s.SeriesId == id);

            //Start removing this series
            _jobProvider.QueueJob(typeof(DeleteSeriesJob), id);

            var series = GetSeriesModels(seriesInDb);
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

        public ActionResult SearchForSeries(string seriesName)
        {
            var model = new List<SeriesSearchResultModel>();

            //Get Results from TvDb and convert them to something we can use.
            foreach (var tvdbSearchResult in _tvDbProvider.SearchSeries(seriesName))
            {
                model.Add(new SeriesSearchResultModel
                              {
                                  TvDbId = tvdbSearchResult.Id,
                                  TvDbName = tvdbSearchResult.SeriesName,
                                  FirstAired = tvdbSearchResult.FirstAired
                              });
            }

            //model.Add(new SeriesSearchResultModel{ TvDbId = 12345, TvDbName = "30 Rock", FirstAired = DateTime.Today });
            //model.Add(new SeriesSearchResultModel { TvDbId = 65432, TvDbName = "The Office (US)", FirstAired = DateTime.Today.AddDays(-100) });

            return PartialView("SeriesSearchResults", model);
        }

        [HttpPost]
        public JsonResult SaveSeason(int seriesId, int seasonNumber, bool monitored)
        {
            if (_episodeProvider.IsIgnored(seriesId, seasonNumber) == monitored)
            {
                _episodeProvider.SetSeasonIgnore(seriesId, seasonNumber, !monitored);
            }

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

            return View(model);
        }

        public ActionResult SyncEpisodesOnDisk(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(DiskScanJob), seriesId);

            return RedirectToAction("Details", new { seriesId });
        }

        public ActionResult UpdateInfo(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(UpdateInfoJob), seriesId);
            return RedirectToAction("Details", new { seriesId });
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
                                                        EpisodeFileCount = s.EpisodeFileCount
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
                    airDate = e.AirDate.Value.ToShortDateString();

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

        private string GetSeasonString(int seasonNumber)
        {
            if (seasonNumber == 0)
                return "Specials";

            return String.Format("Season #{0}", seasonNumber);
        }
    }
}