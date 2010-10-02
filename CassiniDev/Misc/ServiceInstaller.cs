using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
namespace CassiniDev
{

    
    [RunInstaller(true)]
    public sealed class MyServiceInstallerProcess : ServiceProcessInstaller
    {
        public MyServiceInstallerProcess()
        {
            this.Account = ServiceAccount.NetworkService;

        }
    }
    [RunInstaller(true)]
    public sealed class MyServiceInstaller : ServiceInstaller
    {
        public MyServiceInstaller()
        {


            this.Description = "CassiniDev";
            this.DisplayName = "CassiniDev";
            this.ServiceName = "CassiniDev";
            this.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
        }
    }
     
}
