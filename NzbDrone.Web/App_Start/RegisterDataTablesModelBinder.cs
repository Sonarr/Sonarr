using System.Web.Mvc;
using System.Web.WebPages;
using NzbDrone.Web.Helpers;
using NzbDrone.Web.Models;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NzbDrone.Web.App_Start.RegisterDatatablesModelBinder), "Start")]

namespace NzbDrone.Web.App_Start {
    public static class RegisterDatatablesModelBinder {
        public static void Start() {
            if (!ModelBinders.Binders.ContainsKey(typeof(DataTablesParams)))
                ModelBinders.Binders.Add(typeof(DataTablesParams), new DataTablesModelBinder());
        }
    }
}