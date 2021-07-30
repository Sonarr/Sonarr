using Microsoft.AspNetCore.Builder;
using Nancy.Bootstrapper;
using Nancy.Owin;

namespace NzbDrone.Host.Middleware
{
    public class NancyMiddleware : IAspNetCoreMiddleware
    {
        private readonly INancyBootstrapper _nancyBootstrapper;

        public int Order => 2;

        public NancyMiddleware(INancyBootstrapper nancyBootstrapper)
        {
            _nancyBootstrapper = nancyBootstrapper;
        }

        public void Attach(IApplicationBuilder appBuilder)
        {
            var options = new NancyOptions
            {
                Bootstrapper = _nancyBootstrapper,
                PerformPassThrough = context => context.Request.Path.StartsWith("/signalr")
            };

            appBuilder.UseOwin(x => x.UseNancy(options));
        }
    }
}
