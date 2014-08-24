using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using NzbDrone.Common.EnvironmentInfo;
using Owin;

namespace NzbDrone.Host.Owin.MiddleWare
{
    public class NzbDroneVersionMiddleWare : IOwinMiddleWare
    {
        public int Order { get { return 0; } }

        public void Attach(IAppBuilder appBuilder)
        {
            appBuilder.Use(typeof(AddApplicationVersionHeader));
        }
    }

    public class AddApplicationVersionHeader : OwinMiddleware
    {
        public AddApplicationVersionHeader(OwinMiddleware next)
            : base(next)
        {
        }
        public override Task Invoke(IOwinContext context)
        {
            context.Response.Headers.Add("X-ApplicationVersion", new string[] { BuildInfo.Version.ToString() });
            return Next.Invoke(context);
        }
    }
}