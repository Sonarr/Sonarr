using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Announcers;
using NLog;

namespace NzbDrone.Core.Datastore
{
    public class NlogAnnouncer : Announcer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public override void Write(string message, bool escaped)
        {
            logger.Info(message);
        }
    }
}
