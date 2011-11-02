using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Growl.Connector;
using Growl.CoreLibrary;

namespace $rootnamespace$
{
    class GrowlHelper
    {
        public static void simpleGrowl(string title, string message = "")
        {
            GrowlConnector simpleGrowl = new GrowlConnector();
            Growl.Connector.Application thisApp = new Growl.Connector.Application(System.Windows.Forms.Application.ProductName);
            NotificationType simpleGrowlType = new NotificationType("SIMPLEGROWL");
            simpleGrowl.Register(thisApp, new NotificationType[] { simpleGrowlType });
            Notification myGrowl = new Notification(System.Windows.Forms.Application.ProductName, "SIMPLEGROWL", title, title, message);
            simpleGrowl.Notify(myGrowl);
        }
    }
}
