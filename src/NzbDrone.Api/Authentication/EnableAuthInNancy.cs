using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Bootstrapper;
using NzbDrone.Api.Extensions.Pipelines;

namespace NzbDrone.Api.Authentication
{
    public class EnableAuthInNancy : IRegisterNancyPipeline
    {
        private readonly IAuthenticationService _authenticationService;

        public EnableAuthInNancy(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public void Register(IPipelines pipelines)
        {
            pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(_authenticationService, "Sonarr"));
            pipelines.BeforeRequest.AddItemToEndOfPipeline(RequiresAuthentication);
        }

        private Response RequiresAuthentication(NancyContext context)
        {
            Response response = null;

            if (!_authenticationService.IsAuthenticated(context))
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }
    }
}