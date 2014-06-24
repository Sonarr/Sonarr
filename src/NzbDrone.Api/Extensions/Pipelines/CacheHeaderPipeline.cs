using Nancy;
using Nancy.Bootstrapper;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Extensions.Pipelines
{
    public class NzbDroneVersionPipeline : IRegisterNancyPipeline
    {
        public void Register(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToStartOfPipeline(Handle);
        }

        private void Handle(NancyContext context)
        {
            if (!context.Response.Headers.ContainsKey("X-ApplicationVersion"))
            {
                context.Response.Headers.Add("X-ApplicationVersion", BuildInfo.Version.ToString());
            }
        }
    }
}