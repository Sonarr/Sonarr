using System;
using Nancy;
using NzbDrone.Core.Configuration;
using NzbDrone.Http.Ping;
using Sonarr.Http.Extensions;

namespace NzbDrone.Http
{
    public class PingModule : NancyModule
    {
        private readonly IConfigRepository _configRepository;

        public PingModule(IConfigRepository configRepository)
        {
            _configRepository = configRepository;

            Get("/ping", x => GetStatus());
        }

        private Response GetStatus()
        {
            try
            {
                _configRepository.All();
            }
            catch (Exception e)
            {
                return new PingResource
                       {
                           Status = "Error"
                       }.AsResponse(Context, HttpStatusCode.InternalServerError);
            }

            return new PingResource
                   {
                       Status = "OK"
                   }.AsResponse(Context, HttpStatusCode.OK);
        }
    }
}
