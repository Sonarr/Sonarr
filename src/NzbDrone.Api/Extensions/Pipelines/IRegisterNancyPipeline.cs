using Nancy.Bootstrapper;

namespace NzbDrone.Api.Extensions.Pipelines
{
    public interface IRegisterNancyPipeline
    {
        int Order { get; }

        void Register(IPipelines pipelines);
    }
}