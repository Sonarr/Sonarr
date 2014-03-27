using System;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Extensions.Pipelines;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Authentication
{
    public class EnableStatelessAuthInNancy : IRegisterNancyPipeline
    {
        private static String API_KEY;

        public EnableStatelessAuthInNancy(IConfigFileProvider configFileProvider)
        {
            API_KEY = configFileProvider.ApiKey;
        }

        public void Register(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(ValidateApiKey);
        }

        public Response ValidateApiKey(NancyContext context)
        {
            Response response = null;

            var authorizationHeader = context.Request.Headers.Authorization;
            var apiKeyHeader = context.Request.Headers["X-Api-Key"].FirstOrDefault();
            var apiKey = apiKeyHeader.IsNullOrWhiteSpace() ? authorizationHeader : apiKeyHeader;

            if (context.Request.IsApiRequest() && !ValidApiKey(apiKey))
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }

        private bool ValidApiKey(string apiKey)
        {
            if (!API_KEY.Equals(apiKey)) return false;

            return true;
        }
    }
}