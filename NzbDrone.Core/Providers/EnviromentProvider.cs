using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using NLog;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class EnviromentProvider
    {

        public virtual Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public virtual DateTime BuildDateTime
        {
            get
            {
                var fileLocation = Assembly.GetCallingAssembly().Location;
                return new FileInfo(fileLocation).CreationTime;
            }

        }

        public virtual String AppPath
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(HostingEnvironment.ApplicationPhysicalPath))
                {
                    return HostingEnvironment.ApplicationPhysicalPath;
                }
                return Directory.GetCurrentDirectory();
            }
        }

        public virtual String TempPath
        {
            get
            {
                return Path.GetTempPath();
            }
        }

    }
}
