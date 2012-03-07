using System;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Core.Datastore.Migrations;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Controllers
{
    public class HealthController : Controller
    {
        private readonly EnvironmentProvider _environmentProvider;
        private readonly IDatabase _database;

        public HealthController(EnvironmentProvider environmentProvider, IDatabase database)
        {
            _environmentProvider = environmentProvider;
            _database = database;
        }

        [HttpGet]
        public JsonResult Echo()
        {
            var stat = new
                           {
                               Service = _environmentProvider.Version.ToString(),
                               Schema = _database.Fetch<SchemaInfo>().OrderByDescending(c => c.Version).First()
                           };

            return Json(stat, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Exception()
        {
            throw new NotImplementedException();
        }
    }
}