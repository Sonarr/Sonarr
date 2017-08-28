using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using NzbDrone.Common.EnvironmentInfo;
using Owin;

namespace NzbDrone.Host.Owin.MiddleWare
{
    public class NzbDroneVersionMiddleWare : IOwinMiddleWare
    {
        public int Order => 0;

        public void Attach(IAppBuilder appBuilder)
        {
            appBuilder.Use(typeof(AddApplicationVersionHeader));
        }
    }

    public class AddApplicationVersionHeader : OwinMiddleware
    {
        private readonly KeyValuePair<string, string[]> _versionHeader;

        public AddApplicationVersionHeader(OwinMiddleware next)
            : base(next)
        {
            _versionHeader = new KeyValuePair<string, string[]>("X-ApplicationVersion",
               new[] { BuildInfo.Version.ToString() });
        }

        public override async Task Invoke(IOwinContext context)
        {
            context.Response.Headers.Add(_versionHeader);
            await Next.Invoke(context);
        }
    }
}