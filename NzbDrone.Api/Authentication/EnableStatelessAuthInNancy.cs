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
        private readonly IConfigFileProvider _configFileProvider;

        public EnableStatelessAuthInNancy(IConfigFileProvider configFileProvider)
        {
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
            
            if (context.Request.IsApiRequest() && 
                (String.IsNullOrWhiteSpace(apiKey) || !apiKey.Equals(_configFileProvider.ApiKey)))
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }
    }
}