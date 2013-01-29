using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Autofac;
using MongoDB.Driver;
using NzbDrone.Services.Api.Datastore;

namespace NzbDrone.Services.Api
{
    public static class ApiServicesRegistrationExtention
    {
        public static ContainerBuilder RegisterApiServices(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).SingleInstance();

            containerBuilder.Register(c => c.Resolve<Connection>().GetMainDb())
                            .As<MongoDatabase>().SingleInstance();

            return containerBuilder;
        }
    }
}