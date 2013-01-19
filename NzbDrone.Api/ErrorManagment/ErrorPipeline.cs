using System;
using System.Linq;
using NLog;
using Nancy;

namespace NzbDrone.Api.ErrorManagment
{
    public class ErrorPipeline
    {
        private readonly Logger _logger;

        public ErrorPipeline(Logger logger)
        {
            _logger = logger;
        }

        public Response HandleException(NancyContext context, Exception exception)
        {
            if (exception is ApiException)
            {
                _logger.WarnException("API Error", exception);
                return ((ApiException)exception).ToErrorResponse();
            }
            _logger.ErrorException("Unexpected error", exception);
            return null;
        }
    }
}