using Nancy.Bootstrapper;

namespace NzbDrone.Api.Extensions.Pipelines
{
    public interface IRegisterNancyPipeline
    {
        void Register(IPipelines pipelines);
    }
}