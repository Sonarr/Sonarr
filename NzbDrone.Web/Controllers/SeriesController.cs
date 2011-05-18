using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
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
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly RenameProvider _renameProvider;
        private readonly RootDirProvider _rootDirProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly TvDbProvider _tvDbProvider;
        private readonly JobProvider _jobProvider;
        private readonly SeasonProvider _seasonProvider;
        //
        // GET: /Series/

        public SeriesController(SeriesProvider seriesProvider,
                                EpisodeProvider episodeProvider,
                                QualityProvider qualityProvider, MediaFileProvider mediaFileProvider,
                                RenameProvider renameProvider, RootDirProvider rootDirProvider,
                                TvDbProvider tvDbProvider, JobProvider jobProvider,
                                SeasonProvider seasonProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _qualityProvider = qualityProvider;
            _mediaFileProvider = mediaFileProvider;
            _renameProvider = renameProvider;
            _rootDirProvider = rootDirProvider;
            _tvDbProvider = tvDbProvider;
            _jobProvider = jobProvider;
            _seasonProvider = seasonProvider;
        }

        public ActionResult Index()
        {
            var profiles = _qualityProvider.GetAllProfiles();
            ViewData["SelectList"] = new SelectList(profiles, "QualityProfileId", "Name");

            return View();
        }

        public ActionResult RssSync()
        {
            _jobProvider.QueueJob(typeof(RssSyncJob));
            return RedirectToAction("Index");
        }

        public ActionResult LoadEpisodes(int seriesId)
        {
            _episodeProvider.RefreshEpisodeInfo(seriesId);
            return RedirectToAction("Details", new
                                                   {
                                                       seriesId
                                                   });
        }

        public ActionResult SeasonEditor(int seriesId)
        {
            var model =
                _seriesProvider.GetSeries(seriesId).Seasons.Select(s => new SeasonEditModel
                                                                            {
                                                                                SeasonId = s.SeasonId,
                                                                                SeasonNumber = s.SeasonNumber,
                                                                                SeasonString = GetSeasonString(s.SeasonNumber),
                                                                                Monitored = s.Monitored
                                                                            }).OrderBy(s => s.SeasonNumber).ToList();
            return View(model);
        }

        public ActionResult GetSingleSeasonView(SeasonEditModel model)
        {
            return PartialView("SingleSeason", model);
        }

        [GridAction]
        public ActionResult _AjaxSeriesGrid()
        {
            var series = GetSeriesModels(_seriesProvider.GetAllSeries().ToList());

            return View(new GridModel(series));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveAjaxSeriesEditing(int id, string path, bool monitored, bool seasonFolder, int qualityProfileId, List<SeasonEditModel> seasons)
        {
            var oldSeries = _seriesProvider.GetSeries(id);
            oldSeries.Path = path;
            oldSeries.Monitored = monitored;
            oldSeries.SeasonFolder = seasonFolder;
            oldSeries.QualityProfileId = qualityProfileId;

            _seriesProvider.UpdateSeries(oldSeries);

            var series = GetSeriesModels(_seriesProvider.GetAllSeries().ToList());
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
        public ActionResult _AjaxSeasonGrid(int seasonId)
        {
            var episodes = _episodeProvider.GetEpisodeBySeason(seasonId).Select(c => new EpisodeModel
                                                                                         {
                                                                                             EpisodeId = c.EpisodeId,
                                                                                             EpisodeNumber = c.EpisodeNumber,
                                                                                             SeasonNumber = c.SeasonNumber,
                                                                                             Title = c.Title,
                                                                                             Overview = c.Overview,
                                                                                             AirDate = c.AirDate,
                                                                                             Path = GetEpisodePath(c.EpisodeFile),
                                                                                             Quality = c.EpisodeFile == null
                                                                                                     ? String.Empty
                                                                                                     : c.EpisodeFile.Quality.ToString()
                                                                                         });
            return View(new GridModel(episodes));
        }

        [GridAction]
        public ActionResult _CustomBinding(GridCommand command, int seasonId)
        {
            IEnumerable<Episode> data = GetData(command);
            return View(new GridModel
                            {
                                Data = data,
                                Total = data.Count()
                            });
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

        private IEnumerable<Episode> GetData(GridCommand command)
        {
            return null;
            /*    
            IQueryable<Episode> data = .Orders;
            //Apply filtering
            if (command.FilterDescriptors.Any())
            {
                data = data.Where(ExpressionBuilder.Expression<Order>(command.FilterDescriptors));
            }
            // Apply sorting
            foreach (SortDescriptor sortDescriptor in command.SortDescriptors)
            {
                if (sortDescriptor.SortDirection == ListSortDirection.Ascending)
                {
                    switch (sortDescriptor.Member)
                    {
                        case "OrderID":
                            data = data.OrderBy(ExpressionBuilder.Expression<Order, int>(sortDescriptor.Member));
                            break;
                        case "Customer.ContactName":
                            data = data.OrderBy(order => order.Customer.ContactName);
                            break;
                        case "ShipAddress":
                            data = data.OrderBy(order => order.ShipAddress);
                            break;
                        case "OrderDate":
                            data = data.OrderBy(order => order.OrderDate);
                            break;
                    }
                }
                else
                {
                    switch (sortDescriptor.Member)
                    {
                        case "OrderID":
                            data = data.OrderByDescending(order => order.OrderID);
                            break;
                        case "Customer.ContactName":
                            data = data.OrderByDescending(order => order.Customer.ContactName);
                            break;
                        case "ShipAddress":
                            data = data.OrderByDescending(order => order.ShipAddress);
                            break;
                        case "OrderDate":
                            data = data.OrderByDescending(order => order.OrderDate);
                            break;
                    }
                }
            }
            count = data.Count();
            // ... and paging
            if (command.PageSize > 0)
            {
                data = data.Skip((command.Page - 1) * command.PageSize);
            }
            data = data.Take(command.PageSize);
            return data;*/
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveAjaxEditing(string id)
        {
            return RedirectToAction("UnMapped");
        }

        [HttpPost]
        public ActionResult SaveSeasons(List<SeasonEditModel> seasons)
        {
            foreach (var season in seasons)
            {
                var seasonInDb = _seasonProvider.GetSeason(season.SeasonId);
                seasonInDb.Monitored = season.Monitored;
                _seasonProvider.SaveSeason(seasonInDb);
            }

            return Content("Saved");
        }

        public ActionResult Details(int seriesId)
        {
            var series = _seriesProvider.GetSeries(seriesId);
            return View(series);
        }

        public ActionResult SyncEpisodesOnDisk(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(MediaFileScanJob), seriesId);

            return RedirectToAction("Details", new { seriesId });
        }

        public ActionResult UpdateInfo(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(UpdateInfoJob), seriesId);
            return RedirectToAction("Details", new { seriesId });
        }

        public ActionResult RenameAll()
        {
            _renameProvider.RenameAll();
            return RedirectToAction("Index");
        }

        public ActionResult RenameSeries(int seriesId)
        {
            _renameProvider.RenameSeries(seriesId);
            return RedirectToAction("Details", new { seriesId });
        }

        public ActionResult RenameSeason(int seasonId)
        {
            //Todo: Stay of Series Detail... AJAX?
            _renameProvider.RenameSeason(seasonId);
            return RedirectToAction("Index");
        }

        public ActionResult RenameEpisode(int episodeId)
        {
            //Todo: Stay of Series Detail... AJAX?
            _renameProvider.RenameEpisode(episodeId);
            return RedirectToAction("Index");
        }

        //Local Helpers
        private string GetEpisodePath(EpisodeFile file)
        {
            if (file == null)
                return String.Empty;

            //Return the path relative to the Series' Folder
            return file.Path.Replace(file.Series.Path, "").Trim(Path.DirectorySeparatorChar);
        }

        private List<SeriesModel> GetSeriesModels(List<Series> seriesInDb)
        {
            var series = new List<SeriesModel>();

            seriesInDb.ForEach(s => series.Add(new SeriesModel
            {
                SeriesId = s.SeriesId,
                Title = s.Title,
                AirsDayOfWeek = s.AirsDayOfWeek.ToString(),
                Monitored = s.Monitored,
                Overview = s.Overview,
                Path = s.Path,
                QualityProfileId = s.QualityProfileId,
                QualityProfileName = s.QualityProfile.Name,
                SeasonsCount = s.Seasons.Where(x => x.SeasonNumber > 0).Count(),
                SeasonFolder = s.SeasonFolder,
                Status = s.Status
            }));

            return series;
        }

        private string GetSeasonString(int seasonNumber)
        {
            if (seasonNumber == 0)
                return "Specials";

            return String.Format("Season# {0}", seasonNumber);
        }
    }
}