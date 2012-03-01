using System;
using System.Linq;
using System.Web.Mvc;
using NLog;
using Ninject;
using NzbDrone.Common.Contract;
using NzbDrone.Services.Service.Repository.Reporting;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Controllers
{
    public class ExceptionController : Controller
    {
        private readonly IDatabase _database;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ExceptionController(IDatabase database)
        {
            _database = database;
        }

        [HttpPost]
        public EmptyResult ReportExisting(ExistingExceptionReport existingExceptionReport)
        {
            try
            {
                if (ExceptionHashExists(existingExceptionReport.Hash))
                {

                    var exceptionInstance = new ExceptionInstance
                                                {
                                                    ExceptionHash = existingExceptionReport.Hash,
                                                    IsProduction = existingExceptionReport.IsProduction,
                                                    LogMessage = existingExceptionReport.LogMessage,
                                                    Timestamp = DateTime.Now
                                                };

                    _database.Insert(exceptionInstance);
                }
                else
                {
                    logger.Warn("Invalid exception hash '{0}'", existingExceptionReport.Hash);
                }
            }
            catch (Exception e)
            {
                logger.FatalException("Error has occurred while saving exception", e);
                throw;
            }

            return new EmptyResult();
        }

        [HttpPost]
        public JsonResult ReportNew(ExceptionReport exceptionReport)
        {
            try
            {
                var exceptionHash = GetExceptionDetailId(exceptionReport);

                var exceptionInstance = new ExceptionInstance
                                     {
                                         ExceptionHash = exceptionHash,
                                         IsProduction = exceptionReport.IsProduction,
                                         LogMessage = exceptionReport.LogMessage,
                                         Timestamp = DateTime.Now
                                     };

                _database.Insert(exceptionInstance);

                return new JsonResult { Data = new ExceptionReportResponse { ExceptionHash = exceptionHash } };
            }
            catch (Exception e)
            {
                logger.FatalException("Error has occurred while saving exception", e);
                if (!exceptionReport.IsProduction)
                {
                    throw;
                }
            }

            return new JsonResult();
        }

        private string GetExceptionDetailId(ExceptionReport exceptionReport)
        {
            var reportHash = Hash(String.Concat(exceptionReport.Version, exceptionReport.String, exceptionReport.Logger));

            if (!ExceptionHashExists(reportHash))
            {
                var exeptionDetail = new ExceptionDetail();
                exeptionDetail.Hash = reportHash;
                exeptionDetail.Logger = exceptionReport.Logger;
                exeptionDetail.String = exceptionReport.String;
                exeptionDetail.Type = exceptionReport.Type;
                exeptionDetail.Version = exceptionReport.Version;

                _database.Insert(exeptionDetail);
            }

            return reportHash;
        }

        private bool ExceptionHashExists(string reportHash)
        {
            return _database.Exists<ExceptionDetail>(reportHash);
        }

        private static string Hash(string input)
        {
            uint mCrc = 0xffffffff;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            foreach (byte myByte in bytes)
            {
                mCrc ^= ((uint)(myByte) << 24);
                for (var i = 0; i < 8; i++)
                {
                    if ((Convert.ToUInt32(mCrc) & 0x80000000) == 0x80000000)
                    {
                        mCrc = (mCrc << 1) ^ 0x04C11DB7;
                    }
                    else
                    {
                        mCrc <<= 1;
                    }
                }
            }
            return String.Format("{0:x8}", mCrc);
        }
    }
}