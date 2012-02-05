using System;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common.Contract;
using NzbDrone.Services.Service.Repository.Reporting;
using PetaPoco;


namespace NzbDrone.Services.Service.Controllers
{
    public class ReportingController : Controller
    {
        private readonly IDatabase _database;

        private const string OK = "OK";

        public ReportingController(IDatabase database)
        {
            _database = database;
        }

        [HttpPost]
        public JsonResult ParseError(ParseErrorReport parseErrorReport)
        {
            if (ParseErrorExists(parseErrorReport.Title))
                return Json(OK);

            var row = new ParseErrorRow();
            row.LoadBase(parseErrorReport);
            row.Title = parseErrorReport.Title;

            _database.Insert(row);

            return Json(OK);
        }


        private bool ParseErrorExists(string title)
        {
            return _database.Exists<ParseErrorRow>("WHERE Title = @0", title);
        }

        [HttpPost]
        public JsonResult ReportException(ExceptionReport exceptionReport)
        {
            var row = new ExceptionRow();
            row.LoadBase(exceptionReport);
            row.LogMessage = exceptionReport.LogMessage;
            row.Logger = exceptionReport.Logger;
            row.String = exceptionReport.String;
            row.Type = exceptionReport.Type;

            _database.Insert(row);

            return Json(OK);
        }
    }
}