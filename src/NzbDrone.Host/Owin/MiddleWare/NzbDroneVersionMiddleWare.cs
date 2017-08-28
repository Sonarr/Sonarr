using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
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
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(AddApplicationVersionHeader));

        public AddApplicationVersionHeader(OwinMiddleware next)
            : base(next)
        {
            _versionHeader = new KeyValuePair<string, string[]>("X-Application-Version", new[] { BuildInfo.Version.ToString() });
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                context.Response.Headers.Add(_versionHeader);
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                Logger.Debug("Unable to set version header");
            }
        }
    }
}
