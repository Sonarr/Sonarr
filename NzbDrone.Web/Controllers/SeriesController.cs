using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;

namespace NzbDrone.Web.Controllers
{
    public class SeriesController : Controller
    {
        private readonly ISeriesProvider _seriesProvider;
        private readonly IEpisodeProvider _episodeProvider;
        //
        // GET: /Series/

        public SeriesController(ISeriesProvider seriesProvider, IEpisodeProvider episodeProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
        }

        public ActionResult Index()
        {
            ViewData.Model = _seriesProvider.GetSeries().ToList();
            return View();
        }


        public ActionResult Sync()
        {
            _seriesProvider.SyncSeriesWithDisk();
            return RedirectToAction("Index");
        }


        public ActionResult UnMapped()
        {
            _seriesProvider.SyncSeriesWithDisk();
            return View(_seriesProvider.GetUnmappedFolders());
        }


        public ActionResult LoadEpisodes(int seriesId)
        {
            _episodeProvider.RefreshSeries(seriesId);
            return RedirectToAction("Details", new
            {
                seriesId = seriesId
            });
        }

        public JsonResult MediaDetect()
        {
            Core.Providers.IMediaDiscoveryProvider disco = new Core.Providers.MediaDiscoveryProvider();
            return Json(new { Discovered = disco.DiscoveredMedia }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LightUpMedia()
        {
            Core.Providers.IMediaDiscoveryProvider disco = new Core.Providers.MediaDiscoveryProvider();
            IMediaProvider p = disco.Providers[0];
            return Json(new { ID = 0, HTML = "<span class='MediaRenderer XBMC'><span class='Play'>Play</span><span class='Pause'>Pause</span><span class='Stop'>Stop</span></span>" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ControlMedia()
        {
            Core.Providers.IMediaDiscoveryProvider disco = new Core.Providers.MediaDiscoveryProvider();
            IMediaProvider p = disco.Providers[0];
            string action = Request["Action"];
            switch (action)
            {
                case "Play":
                    p.Play();
                    break;
                case "Pause":
                    p.Pause();
                    break;
                case "Stop":
                    p.Stop();
                    break;
                default:
                    break;
            }
            return Json(new { Success=true}, JsonRequestBehavior.AllowGet);
        }
        
        //
        // GET: /Series/Details/5

        public ActionResult Details(int seriesId)
        {
            return View(_seriesProvider.GetSeries(seriesId));
        }


    }
}
