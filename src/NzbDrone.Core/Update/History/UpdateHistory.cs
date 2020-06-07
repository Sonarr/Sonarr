using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Update.History
{
    public class UpdateHistory : ModelBase
    {
        public DateTime Date { get; set; }
        public Version Version { get; set; }
        public UpdateHistoryEventType EventType { get; set; }
    }
}
