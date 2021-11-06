using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Http.Ping;

namespace NzbDrone.Http
{
    public class PingController : Controller
    {
        private readonly IConfigRepository _configRepository;

        public PingController(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        [HttpGet]
        private IActionResult GetStatus()
        {
            try
            {
                _configRepository.All();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new PingResource
                {
                    Status = "Error"
                });
            }

            return StatusCode(StatusCodes.Status200OK, new PingResource
            {
                Status = "OK"
            });
        }
    }
}
