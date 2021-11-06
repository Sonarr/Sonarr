using System.Data.SQLite;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Exceptions;
using Sonarr.Http.Exceptions;

namespace Sonarr.Http.ErrorManagement
{
    public class SonarrErrorPipeline
    {
        private readonly Logger _logger;

        public SonarrErrorPipeline(Logger logger)
        {
            _logger = logger;
        }

        public async Task HandleException(HttpContext context)
        {
            _logger.Trace("Handling Exception");

            var response = context.Response;
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            var statusCode = HttpStatusCode.InternalServerError;
            var errorModel = new ErrorModel
            {
                Message = exception?.Message,
                Description = exception?.ToString()
            };

            if (exception is ApiException apiException)
            {
                _logger.Warn(apiException, "API Error:\n{0}", apiException.Message);

                errorModel = new ErrorModel(apiException);
                statusCode = apiException.StatusCode;
            }
            else if (exception is ValidationException validationException)
            {
                _logger.Warn("Invalid request {0}", validationException.Message);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.ContentType = "application/json";
                await response.WriteAsync(STJson.ToJson(validationException.Errors));
                return;
            }
            else if (exception is NzbDroneClientException clientException)
            {
                statusCode = clientException.StatusCode;
            }
            else if (exception is ModelNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (exception is ModelConflictException)
            {
                statusCode = HttpStatusCode.Conflict;
            }
            else if (exception is SQLiteException sqLiteException)
            {
                if (context.Request.Method == "PUT" || context.Request.Method == "POST")
                {
                    if (sqLiteException.Message.Contains("constraint failed"))
                    {
                        statusCode = HttpStatusCode.Conflict;
                    }
                }

                _logger.Error(sqLiteException, "[{0} {1}]", context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.Fatal(exception, "Request Failed. {0} {1}", context.Request.Method, context.Request.Path);
            }

            await errorModel.WriteToResponse(response, statusCode);
        }
    }
}
