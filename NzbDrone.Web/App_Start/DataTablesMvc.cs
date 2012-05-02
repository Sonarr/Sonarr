using DataTables.Mvc.Core.Helpers;
using DataTables.Mvc.Core.Models;
using System.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NzbDrone.Web.App_Start.DataTablesModelBinderActivator), "Start")]

namespace NzbDrone.Web.App_Start
{
    public static class DataTablesModelBinderActivator
    {
        public static void Start()
        {
            if (!ModelBinders.Binders.ContainsKey(typeof(DataTablesParams)))
                ModelBinders.Binders.Add(typeof(DataTablesParams), new DataTablesModelBinder());
        }
    }
}