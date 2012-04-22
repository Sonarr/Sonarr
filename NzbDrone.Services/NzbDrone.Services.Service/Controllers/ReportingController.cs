using System;
using System.Linq;
using System.Web.Mvc;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Contract;
using NzbDrone.Services.Service.Repository.Reporting;
using Services.PetaPoco;


namespace NzbDrone.Services.Service.Controllers
{
    public class ReportingController : Controller
    {
        private readonly IDatabase _database;
        private readonly ExceptionController _exceptionController;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string OK = "OK";

        public ReportingController(IDatabase database, ExceptionController exceptionController)
        {
            _database = database;
            _exceptionController = exceptionController;
        }

        [HttpPost]
        public JsonResult ParseError(ParseErrorReport parseErrorReport)
        {
            try
            {


                logger.Trace(parseErrorReport.NullSafe());

                if (ParseErrorExists(parseErrorReport.Title))
                    return Json(OK);

                var row = new ParseErrorRow();
                row.LoadBase(parseErrorReport);
                row.Title = parseErrorReport.Title;

                _database.Insert(row);

                return Json(OK);
            }
            catch (Exception e)
            {
                logger.FatalException("Error has occurred while saving parse report", e);
                if (!parseErrorReport.IsProduction)
                {
                    throw;
                }
            }

            return new JsonResult();
        }


        private bool ParseErrorExists(string title)
        {
            return _database.Exists<ParseErrorRow>(title);
        }

        [HttpPost]
        public JsonResult ReportException()
        {
            return new JsonResult();
        }
    }
}