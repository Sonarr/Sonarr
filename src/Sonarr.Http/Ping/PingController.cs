using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Configuration;
using NzbDrone.Http.Ping;

namespace NzbDrone.Http
{
    public class PingController : Controller
    {
        private readonly IConfigRepository _configRepository;
        private readonly ICached<IEnumerable<Config>> _cache;

        public PingController(IConfigRepository configRepository, ICacheManager cacheManager)
        {
            _configRepository = configRepository;
            _cache = cacheManager.GetCache<IEnumerable<Config>>(GetType());
        }

        [AllowAnonymous]
        [HttpGet("/ping")]
        [HttpHead("/ping")]
        [Produces("application/json")]
        public Results<Ok<PingResource>, InternalServerError<PingResource>> GetStatus()
        {
            try
            {
                _cache.Get("ping", _configRepository.All, TimeSpan.FromSeconds(5));
            }
            catch (Exception)
            {
                return TypedResults.InternalServerError(new PingResource
                {
                    Status = "Error"
                });
            }

            return TypedResults.Ok(new PingResource
            {
                Status = "OK"
            });
        }
    }
}
