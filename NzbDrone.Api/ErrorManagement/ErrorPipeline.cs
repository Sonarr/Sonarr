using System;
using System.Linq;
using FluentValidation;
using NLog;
using Nancy;
using NzbDrone.Api.Extensions;

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

            var validationException = exception as ValidationException;

            if (validationException != null)
            {
                _logger.Warn("Invalid request {0}", validationException.Message);


                return validationException.Errors.AsResponse(HttpStatusCode.BadRequest);

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