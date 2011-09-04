using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class MisnamedController : Controller
    {
        private readonly MisnamedProvider _misnamedProvider;

        public MisnamedController(MisnamedProvider misnamedProvider)
        {
            _misnamedProvider = misnamedProvider;
        }

        public ActionResult Index()
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxBinding(GridCommand gridCommand)
        {
            var totalItems = 0;

            var misnamed = _misnamedProvider.MisnamedFiles(gridCommand.Page, gridCommand.PageSize, out totalItems);

            return View(new GridModel{ Data = misnamed, Total = totalItems });
        }
    }
}
