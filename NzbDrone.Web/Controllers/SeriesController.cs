using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
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

namespace NzbDrone.Web.Controllers
{
    [HandleError]
    public class SeriesController : Controller
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly JobProvider _jobProvider;
        private readonly SeasonProvider _seasonProvider;
        //
        // GET: /Series/

        public SeriesController(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
                                QualityProvider qualityProvider, JobProvider jobProvider,
                                SeasonProvider seasonProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _qualityProvider = qualityProvider;
            _jobProvider = jobProvider;
            _seasonProvider = seasonProvider;
        }

        public ActionResult Index()
        {
            var series = GetSeriesModels(_seriesProvider.GetAllSeriesWithEpisodeCount());
            var serialized = new JavaScriptSerializer().Serialize(series);

            return View((object)serialized);
        }

        public ActionResult SingleSeriesEditor(int seriesId)
        {
            var profiles = _qualityProvider.All();
            ViewData["SelectList"] = new SelectList(profiles, "QualityProfileId", "Name");

            var backlogStatusTypes = new List<KeyValuePair<int, string>>();

            foreach (BacklogSettingType backlogStatusType in Enum.GetValues(typeof(BacklogSettingType)))
            {
                backlogStatusTypes.Add(new KeyValuePair<int, string>((int)backlogStatusType, backlogStatusType.ToString()));
            }

            ViewData["BacklogSettingSelectList"] = new SelectList(backlogStatusTypes, "Key", "Value");

            var series = GetSeriesModels(new List<Series>{_seriesProvider.GetSeries(seriesId)}).Single();
            return View(series);
        }

        [HttpPost]
        public EmptyResult SaveSingleSeriesEditor(SeriesModel seriesModel)
        {
            var series = _seriesProvider.GetSeries(seriesModel.SeriesId);
            series.Monitored = seriesModel.Monitored;
            series.SeasonFolder = seriesModel.SeasonFolder;
            series.QualityProfileId = seriesModel.QualityProfileId;
            series.Path = seriesModel.Path;
            series.BacklogSetting = (BacklogSettingType)seriesModel.BacklogSetting;

            _seriesProvider.UpdateSeries(series);

            return new EmptyResult();
        }

        [HttpPost]
        public EmptyResult DeleteSeries(int seriesId)
        {
            _jobProvider.QueueJob(typeof(DeleteSeriesJob), seriesId);

            return new EmptyResult();
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

            var seasons = _seasonProvider.All(seriesId).Select(s => new SeasonModel
                                                                    {
                                                                        SeriesId = seriesId,
                                                                        SeasonNumber = s.SeasonNumber,
                                                                        Ignored = s.Ignored,
                                                                        Episodes = GetEpisodeModels(s.Episodes).OrderByDescending(e => e.EpisodeNumber).ToList(),
                                                                        CommonStatus = GetCommonStatus(s.Episodes)
                                                                    }).ToList();
            model.Seasons = seasons;
  
            return View(model);
        }

        public ActionResult Editor()
        {
            var profiles = _qualityProvider.All();
            ViewData["QualityProfiles"] = profiles;

            //Create the select lists
            var masterProfiles = profiles.ToList();
            masterProfiles.Insert(0, new QualityProfile {QualityProfileId = -10, Name = "Select..."});
            ViewData["MasterProfileSelectList"] = new SelectList(masterProfiles, "QualityProfileId", "Name");

            ViewData["BoolSelectList"] = new SelectList(new List<KeyValuePair<int, string>>
                                                            {
                                                                    new KeyValuePair<int, string>(-10, "Select..."),
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
            masterBacklogList.Insert(0, new KeyValuePair<int, string>(-10, "Select..."));
            ViewData["MasterBacklogSettingSelectList"] = new SelectList(masterBacklogList, "Key", "Value");

            var series = _seriesProvider.GetAllSeries().OrderBy(o => SortHelper.SkipArticles(o.Title));

            return View(series);
        }

        [HttpPost]
        public JsonResult SaveEditor(List<Series> series)
        {
            //Save edits
            if (series == null || series.Count == 0)
                return JsonNotificationResult.Oops("Invalid post data");

            _seriesProvider.UpdateFromSeriesEditor(series);
            return JsonNotificationResult.Info("Series Mass Edit Saved");
        }

        private List<SeriesModel> GetSeriesModels(IList<Series> seriesInDb)
        {
            var series = seriesInDb.Select(s => new SeriesModel
                                                    {
                                                        SeriesId = s.SeriesId,
                                                        Title = s.Title,
                                                        TitleSorter = SortHelper.SkipArticles(s.Title),
                                                        AirsDayOfWeek = s.AirsDayOfWeek.ToString(),
                                                        Monitored = s.Monitored,
                                                        Overview = s.Overview,
                                                        Path = s.Path,
                                                        QualityProfileId = s.QualityProfileId,
                                                        QualityProfileName = s.QualityProfile.Name,
                                                        Network = s.Network,
                                                        SeasonFolder = s.SeasonFolder,
                                                        BacklogSetting = (int)s.BacklogSetting,
                                                        Status = s.Status,
                                                        SeasonsCount = s.SeasonCount,
                                                        EpisodeCount = s.EpisodeCount,
                                                        EpisodeFileCount = s.EpisodeFileCount,
                                                        NextAiring = s.NextAiring == null ? String.Empty : s.NextAiring.Value.ToBestDateString(),
                                                        NextAiringSorter = s.NextAiring == null ? "12/31/9999" : s.NextAiring.Value.ToString("MM/dd/yyyy"),
                                                        AirTime = s.AirTimes
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
                var episodeQuality = "N/A";

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
                                     Status = e.Status.ToString(),
                                     Quality = episodeQuality,
                                     Ignored = e.Ignored
                                 });
            }

            return episodes;
        }

        private string GetCommonStatus(IList<Episode> episodes)
        {
            var commonStatusList = episodes.Select(s => s.Status).Distinct().ToList();
            var commonStatus = commonStatusList.Count > 1 ? "Missing" : commonStatusList.First().ToString();
            return commonStatus;
        }
    }
}