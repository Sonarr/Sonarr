using System;
using System.IO;
using System.Web;
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
            string connectionString = String.Format("Data Source={0};Version=3;",Path.Combine(AppPath, "nzbdrone.db")) ;
            var provider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");

            kernel.Bind<ISeriesController>().To<SeriesController>();
            kernel.Bind<IDiskController>().To<DiskController>();
            kernel.Bind<ITvDbController>().To<TvDbController>();
            kernel.Bind<IConfigController>().To<DbConfigController>();
            kernel.Bind<ILog>().ToMethod(c => LogManager.GetLogger("logger-name"));
            kernel.Bind<IRepository>().ToMethod(c => new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations));
        }


        private static string _appPath;

        public static String AppPath
        {
            get { return new DirectoryInfo(HttpContext.Current.Server.MapPath("\\")).Parent.FullName; }
        }
    }
}
