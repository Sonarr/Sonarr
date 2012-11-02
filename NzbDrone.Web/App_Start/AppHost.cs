using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Funq;
using NzbDrone.Api.RequestModels;
using NzbDrone.Api.Services;
using ServiceStack.WebHost.Endpoints;

namespace NzbDrone.Web.App_Start
{
    public class AppHost : AppHostBase
    {
        public AppHost() //Tell ServiceStack the name and where to find your web services
            : base("NzbDrone API", typeof(QualityProfileService).Assembly) { }

        public override void Configure(Container container)
        {
            SetConfig(new EndpointHostConfig { ServiceStackHandlerFactoryPath = "api" });

            Routes
                .Add<QualityProfileRequest>("{ApiKey}/qualityprofiles")
                .Add<QualityProfileRequest>("{ApiKey}/qualityprofiles/{Id}");
        }
    }
}