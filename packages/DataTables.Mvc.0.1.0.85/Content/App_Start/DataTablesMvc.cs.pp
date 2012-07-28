using DataTables.Mvc.Core.Helpers;
using DataTables.Mvc.Core.Models;
using System.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.DataTablesModelBinderActivator), "Start")]

namespace $rootnamespace$.App_Start
{
    public static class DataTablesModelBinderActivator
    {
        public static void Start()
        {
            if (!ModelBinders.Binders.ContainsKey(typeof(DataTablesPageRequest)))
                ModelBinders.Binders.Add(typeof(DataTablesPageRequest), new DataTablesModelBinder());
        }
    }
}