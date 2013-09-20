using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Bootstrapper;

namespace NzbDrone.Api.Authentication
{
    public interface IEnableBasicAuthInNancy
    {
        void Register(IPipelines pipelines);
    }

    public class EnableBasicAuthInNancy : IEnableBasicAuthInNancy
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

            if (!context.Request.Path.StartsWith("/api/") &&
                context.CurrentUser == null &&
                _authenticationService.Enabled)
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }
    }
}