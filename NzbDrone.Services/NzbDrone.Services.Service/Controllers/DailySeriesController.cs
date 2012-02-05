using System.Linq;
using System.Web.Mvc;
using NzbDrone.Services.Service.Providers;

namespace NzbDrone.Services.Service.Controllers
{
    public class DailySeriesController : Controller
    {
        private readonly DailySeriesProvider _dailySeriesProvider;

        public DailySeriesController(DailySeriesProvider dailySeriesProvider)
        {
            _dailySeriesProvider = dailySeriesProvider;
        }

        [HttpGet]
        [OutputCache(CacheProfile = "Cache1Hour")]
        public JsonResult All()
        {
            var all = _dailySeriesProvider.All();

            return Json(all, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [OutputCache(CacheProfile = "Cache1Hour")]
        public JsonResult AllIds()
        {
            var all = _dailySeriesProvider.AllSeriesIds();

            return Json(all, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [OutputCache(CacheProfile = "Cache1HourVaryBySeriesId")]
        public JsonResult Check(int seriesId)
        {
            var result = _dailySeriesProvider.IsDaily(seriesId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}