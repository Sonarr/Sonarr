using System;
using System.Linq;
using NLog;
using Nancy;
using NzbDrone.Api.Extentions;

namespace NzbDrone.Api.ErrorManagement
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
            var apiException = exception as ApiException;

            if (apiException != null)
            {
                _logger.WarnException("API Error", apiException);
                return apiException.ToErrorResponse();
            }

            _logger.ErrorException("Unexpected error", exception);


            return new ErrorModel()
                {
                        Message = exception.Message,
                        Description = exception.ToString()
                }.AsResponse(HttpStatusCode.InternalServerError);
        }
    }
}