using System;
using log4net;
using Ninject;
using NzbDrone.Core.Controllers;
using SubSonic.DataProviders;
using SubSonic.Repository;

namespace NzbDrone.Core
{
    public static class Main
    {

        public static void BindKernel(IKernel kernel)
        {
            var provider = ProviderFactory.GetProvider("Data Source=filename;Version=3;", "System.Data.SQLite");


            kernel.Bind<ISeriesController>().To<SeriesController>();
            kernel.Bind<IDiskController>().To<DiskController>();
            kernel.Bind<ITvDbController>().To<TvDbController>();
            kernel.Bind<IConfigController>().To<DbConfigController>();
            kernel.Bind<ILog>().ToMethod(c => LogManager.GetLogger("logger-name"));
            kernel.Bind<IRepository>().ToMethod(c => new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations));
        }


        public static String AppPath
        {
            get { throw new ApplicationException(); }

        }

    }
}
