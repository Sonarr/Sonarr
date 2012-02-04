using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using MvcMiniProfiler;
using NzbDrone.Common.Model;
using NzbDrone.Core;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
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

            var backlogStatusTypes = new List<KeyValuePair<int, string>>();

            foreach (BacklogSettingType backlogStatusType in Enum.GetValues(typeof(BacklogSettingType)))
            {
                backlogStatusTypes.Add(new KeyValuePair<int, string>((int)backlogStatusType, backlogStatusType.ToString()));
            }

            ViewData["BacklogSettingSelectList"] = new SelectList(backlogStatusTypes, "Key", "Value");

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
        public ActionResult _SaveAjaxSeriesEditing(int id, string path, bool monitored, bool seasonFolder, int qualityProfileId, int backlogSetting)
        {
            var oldSeries = _seriesProvider.GetSeries(id);
            oldSeries.Monitored = monitored;
            oldSeries.SeasonFolder = seasonFolder;
            oldSeries.QualityProfileId = qualityProfileId;
            oldSeries.Path = path;
            oldSeries.BacklogSetting = (BacklogSettingType)backlogSetting;

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
        public EmptyResult SaveSeasonIgnore(int seriesId, int seasonNumber, bool ignored)
        {
            _episodeProvider.SetSeasonIgnore(seriesId, seasonNumber, ignored);
            return new EmptyResult();
        }

        [HttpPost]
        public EmptyResult SaveEpisodeIgnore(int episodeId, bool ignored)
        {
            _episodeProvider.SetEpisodeIgnore(episodeId, ignored);
            return new EmptyResult();
        }

        public ActionResult Details2(int seriesId)
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

        public ActionResult Details(int seriesId)
        {
            var series = _seriesProvider.GetSeries(seriesId);

            var model = new SeriesDetailsModel();

            if (series.AirsDayOfWeek != null)
            {
                model.AirsDayOfWeek = series.AirsDayOfWeek.Value.ToString();
            }
            else
            {
                model.AirsDayOfWeek = "N/A";
            }
            model.Overview = series.Overview;
            model.Title = series.Title;
            model.SeriesId = series.SeriesId;
            model.HasBanner = !String.IsNullOrEmpty(series.BannerUrl);

            var seasons = new List<SeasonModel>();
            var episodes = _episodeProvider.GetEpisodeBySeries(seriesId);

            foreach (var season in episodes.Select(s => s.SeasonNumber).Distinct())
            {
                seasons.Add(new SeasonModel
                                      {
                                              SeasonNumber = season,
                                              Episodes = GetEpisodeModels(episodes.Where(e => e.SeasonNumber == season).ToList()).OrderByDescending(e=> e.EpisodeNumber).ToList()
                                      });
            }

            model.Seasons = seasons;
  
            return View(model);
        }

        public ActionResult MassEdit()
        {
            var profiles = _qualityProvider.All();
            ViewData["QualityProfiles"] = profiles;

            //Create the select lists
            var masterProfiles = profiles.ToList();
            masterProfiles.Insert(0, new QualityProfile {QualityProfileId = -10, Name = "Unchanged"});
            ViewData["MasterProfileSelectList"] = new SelectList(masterProfiles, "QualityProfileId", "Name");

            ViewData["BoolSelectList"] = new SelectList(new List<KeyValuePair<int, string>>
                                                            {
                                                                    new KeyValuePair<int, string>(-10, "Unchanged"),
                                                                    new KeyValuePair<int, string>(1, "True"),
                                                                    new KeyValuePair<int, string>(0, "False")
                                                            }, "Key", "Value"
                                                        );

            var backlogSettingTypes = new List<KeyValuePair<int, string>>();

            foreach (BacklogSettingType backlogSettingType in Enum.GetValues(typeof(BacklogSettingType)))
            {
                backlogSettingTypes.Add(new KeyValuePair<int, string>((int)backlogSettingType, backlogSettingType.ToString()));
            }

            ViewData["BacklogSettingTypes"] = backlogSettingTypes;

            var masterBacklogList = backlogSettingTypes.ToList();
            masterBacklogList.Insert(0, new KeyValuePair<int, string>(-10, "Unchanged"));
            ViewData["MasterBacklogSettingSelectList"] = new SelectList(masterBacklogList, "Key", "Value");

            var series = _seriesProvider.GetAllSeries().OrderBy(o => SortHelper.SkipArticles(o.Title));

            return View(series);
        }

        [HttpPost]
        public JsonResult SaveMassEdit(List<Series> series)
        {
            //Save edits
            if (series == null || series.Count == 0)
                return JsonNotificationResult.Opps("Invalid post data");

            _seriesProvider.UpdateFromMassEdit(series);
            return JsonNotificationResult.Info("Series Mass Edit Saved");
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
                                                        BacklogSetting = (int)s.BacklogSetting,
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