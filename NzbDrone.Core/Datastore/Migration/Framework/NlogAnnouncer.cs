using FluentMigrator.Runner.Announcers;
using NLog;

namespace NzbDrone.Core.Datastore.Migration.Framework
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
