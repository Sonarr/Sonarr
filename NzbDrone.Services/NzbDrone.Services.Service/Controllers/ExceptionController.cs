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
    public class ExceptionController : Controller
    {
        private readonly IDatabase _database;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string OK = "OK";

        public ExceptionController(IDatabase database)
        {
            _database = database;
        }

        [HttpPost]
        public JsonResult ReportNew(ExceptionReport exceptionReport)
        {
            try
            {
                var exceptionId = GetExceptionDetailId(exceptionReport);

                var exceptionInstance = new ExceptionInstance
                                     {
                                         ExceptionDetail = exceptionId,
                                         IsProduction = exceptionReport.IsProduction,
                                         LogMessage = exceptionReport.LogMessage,
                                         Timestamp = DateTime.Now
                                     };

                _database.Insert(exceptionInstance);

                return new JsonResult { Data = new ExceptionReportResponse { ExceptionId = exceptionId } };
            }
            catch (Exception e)
            {
                logger.FatalException("Error has occurred while logging exception", e);
                throw;
            }
        }


        private int GetExceptionDetailId(ExceptionReport exceptionReport)
        {
            var reportHash = Hash(exceptionReport.Version + exceptionReport.String + exceptionReport.Logger);
            var id = _database.FirstOrDefault<int>("SELECT Id FROM Exceptions WHERE Hash =@0", reportHash);

            if (id == 0)
            {
                var exeptionDetail = new ExceptionDetail();
                exeptionDetail.Hash = reportHash;
                exeptionDetail.Logger = exceptionReport.Logger;
                exeptionDetail.String = exceptionReport.String;
                exeptionDetail.Type = exceptionReport.Type;
                exeptionDetail.Version = exceptionReport.Version;

                id = Convert.ToInt32(_database.Insert(exeptionDetail));
            }

            return id;
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