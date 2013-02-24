using System.Linq;
using NzbDrone.Core.Datastore;
using PetaPoco;

namespace NzbDrone.Core.ExternalNotification
{
    public class ExternalNotificationDefinition : ModelBase
    {
        public string Name { get; set; }
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnRename { get; set; }
    }
}