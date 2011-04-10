using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;
using TvdbLib.Data;
using EpisodeModel = NzbDrone.Web.Models.EpisodeModel;

namespace NzbDrone.Web.Controllers
{
    [HandleError]
    public class SeriesController : Controller
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly ISyncProvider _syncProvider;
        private readonly RssSyncProvider _rssSyncProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly IMediaFileProvider _mediaFileProvider;
        private readonly RenameProvider _renameProvider;
        private readonly RootDirProvider _rootDirProvider;
        private readonly TvDbProvider _tvDbProvider;
        //
        // GET: /Series/

        public SeriesController(ISyncProvider syncProvider, SeriesProvider seriesProvider,
            EpisodeProvider episodeProvider, RssSyncProvider rssSyncProvider,
            QualityProvider qualityProvider, IMediaFileProvider mediaFileProvider,
            RenameProvider renameProvider, RootDirProvider rootDirProvider,
            TvDbProvider tvDbProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _syncProvider = syncProvider;
            _rssSyncProvider = rssSyncProvider;
            _qualityProvider = qualityProvider;
            _mediaFileProvider = mediaFileProvider;
            _renameProvider = renameProvider;
            _rootDirProvider = rootDirProvider;
            _tvDbProvider = tvDbProvider;
        }

        public ActionResult Index()
        {
            ViewData.Model = _seriesProvider.GetAllSeries().ToList();
            return View();
        }





        public ActionResult RssSync()
        {
            _rssSyncProvider.Begin();
            return RedirectToAction("Index");
        }

        public ActionResult UnMapped(string path)
        {
            return View(_syncProvider.GetUnmappedFolders(path).Select(c => new MappingModel() { Id = 1, Path = c }).ToList());
        }

        public ActionResult LoadEpisodes(int seriesId)
        {
            _episodeProvider.RefreshEpisodeInfo(seriesId);
            return RedirectToAction("Details", new
            {
                seriesId = seriesId
            });
        }

        [GridAction]
        public ActionResult _AjaxSeasonGrid(int seasonId)
        {
            var episodes = _episodeProvider.GetEpisodeBySeason(seasonId).Select(c => new EpisodeModel()
                                                                                     {
                                                                                         EpisodeId = c.EpisodeId,
                                                                                         EpisodeNumber = c.EpisodeNumber,
                                                                                         SeasonNumber = c.SeasonNumber,
                                                                                         Title = c.Title,
                                                                                         Overview = c.Overview,
                                                                                         AirDate = c.AirDate,
                                                                                         Path = GetEpisodePath(c.EpisodeFile),
                                                                                         Quality = c.EpisodeFile == null ? String.Empty : c.EpisodeFile.Quality.ToString()

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

        [GridAction]
        public ActionResult _AjaxUnmappedFoldersGrid()
        {
            var unmappedList = new List<AddExistingSeriesModel>();

            foreach (var folder in _rootDirProvider.GetAll())
            {
                foreach (var unmappedFolder in _syncProvider.GetUnmappedFolders(folder.Path))
                {
                    var tvDbSeries = _seriesProvider.MapPathToSeries(unmappedFolder);

                    //We still want to show this series as unmapped, but we don't know what it will be when mapped
                    //Todo: Provide the user with a way to manually map a folder to a TvDb series (or make them rename the folder...)
                    if (tvDbSeries == null)
                        tvDbSeries = new TvdbSeries { Id = 0, SeriesName = String.Empty };

                    unmappedList.Add(new AddExistingSeriesModel
                                            {
                                                IsWanted = true,
                                                Path = unmappedFolder,
                                                PathEncoded = Url.Encode(unmappedFolder),
                                                TvDbId = tvDbSeries.Id,
                                                TvDbName = tvDbSeries.SeriesName
                                            });
                }
            }

            return View(new GridModel(unmappedList));
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

        public ActionResult Details(int seriesId)
        {
            var series = _seriesProvider.GetSeries(seriesId);
            return View(series);
        }

        public ActionResult Edit(int seriesId)
        {
            var profiles = _qualityProvider.GetAllProfiles();
            ViewData["SelectList"] = new SelectList(profiles, "QualityProfileId", "Name");

            var series = _seriesProvider.GetSeries(seriesId);
            return View(series);
        }

        [HttpPost]
        public ActionResult Edit(Series series)
        {
            //Need to add seriesProvider.Update
            _seriesProvider.UpdateSeries(series);
            return Content("Series Updated Successfully");
        }

        public ActionResult Delete(int seriesId)
        {
            //Need to add seriesProvider.Update
            _seriesProvider.DeleteSeries(seriesId);

            return RedirectToAction("Index", "Series");
        }

        public ActionResult SyncEpisodesOnDisk(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            var series = _seriesProvider.GetSeries(seriesId);
            _mediaFileProvider.Scan(series);

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
    }
}
