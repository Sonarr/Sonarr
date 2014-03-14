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
        private readonly IAuthenticationService _authenticationService;
        private static String API_KEY;

        public EnableStatelessAuthInNancy(IAuthenticationService authenticationService, IConfigFileProvider configFileProvider)
        {
            _authenticationService = authenticationService;
            API_KEY = configFileProvider.ApiKey;
        }

        public void Register(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(ValidateApiKey);
        }

        public Response ValidateApiKey(NancyContext context)
        {
            Response response = null;
            
            if (!RuntimeInfo.IsProduction && context.Request.IsLocalRequest())
            {
                return response;
            }

            var authorizationHeader = context.Request.Headers.Authorization;
            var apiKeyHeader = context.Request.Headers["X-Api-Key"].FirstOrDefault();
            var apiKey = apiKeyHeader.IsNullOrWhiteSpace() ? authorizationHeader : apiKeyHeader;

            if (context.Request.IsApiRequest() && !ValidApiKey(apiKey) && !IsAuthenticated(context))
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }

        private bool ValidApiKey(string apiKey)
        {
            if (apiKey.IsNullOrWhiteSpace()) return false;
            if (!apiKey.Equals(API_KEY)) return false;

            return true;
        }

        private bool IsAuthenticated(NancyContext context)
        {
            return _authenticationService.Enabled && _authenticationService.IsAuthenticated(context);
        }
    }
}