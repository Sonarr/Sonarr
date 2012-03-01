using NzbDrone.Services.Service.Datastore;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Mvc;
using Services.PetaPoco;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NzbDrone.Services.Service.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(NzbDrone.Services.Service.App_Start.NinjectMVC3), "Stop")]

namespace NzbDrone.Services.Service.App_Start
{

    public static class NinjectMVC3
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
            DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<IDatabase>().ToMethod(c => Connection.GetPetaPocoDb());
            return kernel;
        }
    }
}
