using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Autofac;
using Funq;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Api.QualityType;
using ServiceStack.WebHost.Endpoints;
using QualityProfileService = NzbDrone.Api.QualityProfiles.QualityProfileService;

namespace NzbDrone.Api
{
    public class AppHost : AppHostBase
    {
        private IContainer _container;

        public AppHost(IContainer container) //Tell ServiceStack the name and where to find your web services
            : base("NzbDrone API", typeof(QualityProfileService).Assembly)
        {
            _container = container;
        }

        public override void Configure(Container container)
        {
            container.Adapter = new AutofacIocAdapter(_container);
            SetConfig(new EndpointHostConfig { ServiceStackHandlerFactoryPath = "api" });

            Routes
                .Add<QualityProfileModel>("/qualityprofiles")
                .Add<QualityProfileModel>("/qualityprofiles/{Id}")
                .Add<QualityTypeModel>("/qualitytypes")
                .Add<QualityTypeModel>("/qualitytypes/{Id}");

            Bootstrapper.Initialize();
        }
    }
}