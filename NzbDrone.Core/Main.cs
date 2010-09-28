using System;
using System.IO;
using System.Web;
using log4net;
using Ninject;
using NzbDrone.Core.Providers;
using SubSonic.DataProviders;
using SubSonic.Repository;

namespace NzbDrone.Core
{
    public static class Main
    {

        public static void BindKernel(IKernel kernel)
        {
            string connectionString = String.Format("Data Source={0};Version=3;", Path.Combine(AppPath, "nzbdrone.db"));
            var provider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");

            kernel.Bind<ISeriesProvider>().To<SeriesProvider>();
            kernel.Bind<IDiskProvider>().To<DiskProvider>();
            kernel.Bind<ITvDbProvider>().To<TvDbProvider>();
            kernel.Bind<IConfigProvider>().To<ConfigProvider>();
            kernel.Bind<ILog>().ToMethod(c => LogManager.GetLogger("logger-name"));
            kernel.Bind<IRepository>().ToMethod(c => new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations));
        }

        public static String AppPath
        {
            get { return new DirectoryInfo(HttpContext.Current.Server.MapPath("\\")).Parent.FullName; }
        }
    }
}
