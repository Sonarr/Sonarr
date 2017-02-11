using Nancy.Bootstrapper;

namespace Sonarr.Http.Extensions.Pipelines
{
    public interface IRegisterNancyPipeline
    {
        int Order { get; }

        void Register(IPipelines pipelines);
    }
}