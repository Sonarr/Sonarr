using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Authentication
{
    public interface IEnableStatelessAuthInNancy
    {
        void Register(IPipelines pipelines);
    }

    public class EnableStatelessAuthInNancy : IEnableStatelessAuthInNancy
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
            var apiKey = context.Request.Headers["ApiKey"].FirstOrDefault();

            if (!RuntimeInfo.IsProduction &&
                (context.Request.UserHostAddress.Equals("localhost") ||
                context.Request.UserHostAddress.Equals("127.0.0.1") ||
                context.Request.UserHostAddress.Equals("::1")))
            {
                return response;
            }
            
            if (context.Request.Path.StartsWith("/api/") && 
                (apiKey == null || !apiKey.Equals(_configFileProvider.ApiKey)))
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }
    }
}