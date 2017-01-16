using Nancy.Bootstrapper;
using Nancy.Owin;
using Owin;

namespace NzbDrone.Host.Owin.MiddleWare
{
    public class NancyMiddleWare : IOwinMiddleWare
    {
        private readonly INancyBootstrapper _nancyBootstrapper;

        public NancyMiddleWare(INancyBootstrapper nancyBootstrapper)
        {
            _nancyBootstrapper = nancyBootstrapper;
        }

        public int Order => 2;

        public void Attach(IAppBuilder appBuilder)
        {
            var options = new NancyOptions
            {
                Bootstrapper = _nancyBootstrapper,
                PerformPassThrough = context => context.Request.Path.StartsWith("/signalr")
            };

            appBuilder.UseNancy(options);
        }
    }
}