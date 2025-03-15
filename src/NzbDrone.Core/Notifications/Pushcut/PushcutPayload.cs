using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public class PushcutPayload
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public bool? IsTimeSensitive { get; set; }
        public string Image { get; set; }
        public List<PushcutAction> Actions;
    }

    public class PushcutAction
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
