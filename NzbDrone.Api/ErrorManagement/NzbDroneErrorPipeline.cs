using System;
using FluentValidation;
using NLog;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Exceptions;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace NzbDrone.Api.ErrorManagement
{
    public class NzbDroneErrorPipeline
    {
        private readonly Logger _logger;

        public NzbDroneErrorPipeline(Logger logger)
        {
            _logger = logger;
        }

        public Response HandleException(NancyContext context, Exception aggregateException)
        {
            var innerException = (aggregateException.InnerException).InnerException;

            var apiException = innerException as ApiException;

            if (apiException != null)
            {
                _logger.WarnException("API Error", apiException);
                return apiException.ToErrorResponse();
            }

            var validationException = innerException as ValidationException;

            if (validationException != null)
            {
                _logger.Warn("Invalid request {0}", validationException.Message);

                return validationException.Errors.AsResponse(HttpStatusCode.BadRequest);
            }

            var clientException = innerException as NzbDroneClientException;

            if (clientException != null)
            {
                return new ErrorModel
                {
                    Message = innerException.Message,
                    Description = innerException.ToString()
                }.AsResponse((HttpStatusCode)clientException.StatusCode);
            }

            _logger.FatalException("Request Failed", innerException);

            return new ErrorModel
                {
                    Message = innerException.Message,
                    Description = innerException.ToString()
                }.AsResponse(HttpStatusCode.InternalServerError);
        }
    }
}