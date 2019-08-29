using Nancy;
using Nancy.Responses.Negotiation;

namespace Sonarr.Http
{
    public abstract class SonarrModule : NancyModule
    {
        protected SonarrModule(string resource)
        : base(resource)
        {
        }

        protected Negotiator ResponseWithCode(object model, HttpStatusCode statusCode)
        {
            return Negotiate.WithModel(model).WithStatusCode(statusCode);
        }
    }
}
