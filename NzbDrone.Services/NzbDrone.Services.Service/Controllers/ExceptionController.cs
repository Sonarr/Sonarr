using System;
using System.Linq;
using System.Web.Mvc;
using MongoDB.Driver;
using NLog;
using Ninject;
using NzbDrone.Common.Contract;
using NzbDrone.Services.Service.Exceptions;
using NzbDrone.Services.Service.Repository.Reporting;
using Services.PetaPoco;
using ExceptionInstance = NzbDrone.Services.Service.Repository.Reporting.ExceptionInstance;
using ExceptionReport = NzbDrone.Common.Contract.ExceptionReport;

namespace NzbDrone.Services.Service.Controllers
{
    public class ExceptionController : Controller
    {
        private readonly IDatabase _database;
        private readonly ExceptionRepository _exceptionRepository;
        private readonly MongoDatabase _mongoDatabase;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ExceptionController(IDatabase database, ExceptionRepository exceptionRepository)
        {
            _database = database;
            _exceptionRepository = exceptionRepository;
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
                                                    UGuid = existingExceptionReport.UGuid,
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

                var report = new Exceptions.ExceptionReport();
                report.AppVersion = exceptionReport.Version;
                report.ApplicationId = "NzbDrone";
                report.ExceptionMessage = exceptionReport.String;
                report.ExceptionType = exceptionReport.Type;
                report.Location = exceptionReport.Logger;
                report.Message = exceptionReport.LogMessage;
                report.Stack = exceptionReport.String;
                report.Uid = exceptionReport.UGuid.ToString();

                var exceptionHash = _exceptionRepository.Store(report);

                //var exceptionHash = GetExceptionDetailId(exceptionReport);

                //var exceptionInstance = new ExceptionInstance
                //                     {
                //                         ExceptionHash = exceptionHash,
                //                         IsProduction = exceptionReport.IsProduction,
                //                         LogMessage = exceptionReport.LogMessage,
                //                         Timestamp = DateTime.Now,
                //                         UGuid = exceptionReport.UGuid
                //                     };

                //_database.Insert(exceptionInstance);

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