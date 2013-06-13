using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class ProwlSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "API Key", HelpText = "API Key for Prowl")]
        public String ApiKey { get; set; }

        [FieldDefinition(1, Label = "Priority", HelpText = "Priority to send messages at", Type = FieldType.Select, SelectOptions= typeof(ProwlPriority) )]
        public Int32 Priority { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ApiKey) && Priority != null & Priority >= -2 && Priority <= 2;
            }
        }
    }
}
