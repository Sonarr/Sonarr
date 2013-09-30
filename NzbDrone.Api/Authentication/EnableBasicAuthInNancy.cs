using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Bootstrapper;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Extensions.Pipelines;

namespace NzbDrone.Api.Authentication
{
    public class EnableBasicAuthInNancy : IRegisterNancyPipeline
    {
        private readonly IAuthenticationService _authenticationService;

        public EnableBasicAuthInNancy(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public void Register(IPipelines pipelines)
        {
            pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(_authenticationService, "NzbDrone"));
            pipelines.BeforeRequest.AddItemToEndOfPipeline(RequiresAuthentication);
        }

        private Response RequiresAuthentication(NancyContext context)
        {
            Response response = null;

            if (!context.Request.IsApiRequest() && !_authenticationService.IsAuthenticated(context))
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }
    }
}