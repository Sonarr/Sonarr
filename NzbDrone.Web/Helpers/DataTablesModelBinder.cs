using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Helpers
{
    /// <summary>
    /// Model binder for datatables.js parameters a la http://geeksprogramando.blogspot.com/2011/02/jquery-datatables-plug-in-with-asp-mvc.html
    /// </summary>
    public class DataTablesModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            DataTablesParams obj = new DataTablesParams();
            var request = controllerContext.HttpContext.Request.Params;

            obj.iDisplayStart = Convert.ToInt32(request["iDisplayStart"]);
            obj.iDisplayLength = Convert.ToInt32(request["iDisplayLength"]);
            obj.iColumns = Convert.ToInt32(request["iColumns"]);
            obj.sSearch = request["sSearch"];
            obj.bEscapeRegex = Convert.ToBoolean(request["bEscapeRegex"]);
            obj.iSortingCols = Convert.ToInt32(request["iSortingCols"]);
            obj.sEcho = int.Parse(request["sEcho"]);

            for (int i = 0; i < obj.iColumns; i++)
            {
                obj.bSortable.Add(Convert.ToBoolean(request["bSortable_" + i]));
                obj.bSearchable.Add(Convert.ToBoolean(request["bSearchable_" + i]));
                obj.sSearchColumns.Add(request["sSearch_" + i]);
                obj.bEscapeRegexColumns.Add(Convert.ToBoolean(request["bEscapeRegex_" + i]));
                obj.iSortCol.Add(Convert.ToInt32(request["iSortCol_" + i]));
                obj.sSortDir.Add(request["sSortDir_" + i]);
            }
            return obj;
        }
    }
}