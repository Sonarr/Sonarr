using System;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Extensions.Pipelines;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Authentication
{
    public class EnableStatelessAuthInNancy : IRegisterNancyPipeline
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IConfigFileProvider _configFileProvider;

        public EnableStatelessAuthInNancy(IAuthenticationService authenticationService, IConfigFileProvider configFileProvider)
        {
            _authenticationService = authenticationService;
            _configFileProvider = configFileProvider;
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

            var apiKey = context.Request.Headers.Authorization;
            
            if (context.Request.IsApiRequest() && !ValidApiKey(apiKey) && !_authenticationService.IsAuthenticated(context))
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }

        private bool ValidApiKey(string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey)) return false;
            if (!apiKey.Equals(_configFileProvider.ApiKey)) return false;

            return true;
        }
    }
}