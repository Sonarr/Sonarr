﻿using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Host;
using NzbDrone.SysTray;

namespace NzbDrone
{
    public static class WindowsApp
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(WindowsApp));

        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            try
            {
                var startupArgs = new StartupContext(args);

                NzbDroneLogger.Register(startupArgs, false, true);

                Bootstrap.Start(args, e => { e.ConfigureServices((_, s) => s.AddSingleton<IHostedService, SystemTrayApp>()); });
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "EPIC FAIL");
                var message = string.Format("{0}: {1}", e.GetType().Name, e.ToString());
                MessageBox.Show($"{e.GetType().Name}: {e.Message}", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error, caption: "Epic Fail!");
            }
        }
    }
}
