using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    [HandleError]
    public class SeriesController : Controller
    {
        private readonly ISeriesProvider _seriesProvider;
        private readonly IEpisodeProvider _episodeProvider;
        private readonly ISyncProvider _syncProvider;
        private readonly IRssSyncProvider _rssSyncProvider;
        private readonly IQualityProvider _qualityProvider;
        //
        // GET: /Series/

        public SeriesController(ISyncProvider syncProvider, ISeriesProvider seriesProvider, IEpisodeProvider episodeProvider, IRssSyncProvider rssSyncProvider, IQualityProvider qualityProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _syncProvider = syncProvider;
            _rssSyncProvider = rssSyncProvider;
            _qualityProvider = qualityProvider;
        }

        public ActionResult Index()
        {
            ViewData.Model = _seriesProvider.GetAllSeries().ToList();
            return View();
        }

        public ActionResult Sync()
        {
            _syncProvider.BeginSyncUnmappedFolders();
            return RedirectToAction("Index");
        }

        public ActionResult RssSync()
        {
            _rssSyncProvider.Begin();
            return RedirectToAction("Index");
        }

        public ActionResult UnMapped()
        {
            return View(_seriesProvider.GetUnmappedFolders().Select(c => new MappingModel() { Id = 1, Path = c.Value }).ToList());
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
                                                                                         AirDate = c.AirDate
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

        //
        // GET: /Series/Details/5

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
    }
}
