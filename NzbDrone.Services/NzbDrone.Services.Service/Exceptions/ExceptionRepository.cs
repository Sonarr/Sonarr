using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace NzbDrone.Services.Service.Exceptions
{
    public class ExceptionRepository
    {
        private readonly MongoDatabase _mongoDb;

        public ExceptionRepository(MongoDatabase mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public ExceptionRepository()
        {
            
        }

        public virtual string Store(NzbDrone.Services.Service.Exceptions.ExceptionReport exceptionReport)
        {
            var hash = GetExceptionDetailId(exceptionReport);

            var exceptionInstance = new NzbDrone.Services.Service.Exceptions.ExceptionInstance
                                        {
                                            AppVersion = exceptionReport.AppVersion,
                                            ExceptionMessage = exceptionReport.ExceptionMessage,
                                            Message = exceptionReport.Message,
                                            Time = DateTime.UtcNow,
                                            UserId = exceptionReport.Uid
                                        };



            var applicationExceptions = _mongoDb.GetCollection(exceptionReport.ApplicationId);

            applicationExceptions.Update(Query.EQ("_id", hash), Update.PushWrapped("inst", exceptionInstance));

            return hash;
        }

        private string GetExceptionDetailId(NzbDrone.Services.Service.Exceptions.ExceptionReport exceptionReport)
        {
            var hash = Hash(String.Concat(exceptionReport.AppVersion, exceptionReport.Location, exceptionReport.ExceptionType, exceptionReport.Stack));

            if (!ExceptionInfoExists(exceptionReport.ApplicationId, hash))
            {
                var exceptionInfo = new NzbDrone.Services.Service.Exceptions.ExceptionInfo
                                         {
                                             Hash = hash,
                                             Stack = exceptionReport.Stack,
                                             ExceptionType = exceptionReport.ExceptionType,
                                             Location = exceptionReport.Location
                                         };

                _mongoDb.GetCollection(exceptionReport.ApplicationId).Insert(exceptionInfo);
            }

            return hash;
        }

        public bool ExceptionInfoExists(string applicationId, string hash)
        {
            var appCollection = _mongoDb.GetCollection(applicationId);
            return appCollection.FindAs<NzbDrone.Services.Service.Exceptions.ExceptionInfo>(Query.EQ("_id", hash)).Any();
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