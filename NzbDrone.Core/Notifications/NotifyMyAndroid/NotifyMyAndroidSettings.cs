using System;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.NotifyMyAndroid
{
    public class NotifyMyAndroidSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "API Key", HelpLink = "http://www.notifymyandroid.com/")]
        public String ApiKey { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(NotifyMyAndroidPriority))]
        public Int32 Priority { get; set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(ApiKey) && Priority != null & Priority >= -1 && Priority <= 2;
            }
        }
    }
}
