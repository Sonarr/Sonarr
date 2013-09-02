using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.ProgressMessaging
{
    public class NewProgressMessageEvent : IEvent
    {
        public ProgressMessage ProgressMessage { get; set; }

        public NewProgressMessageEvent(ProgressMessage progressMessage)
        {
            ProgressMessage = progressMessage;
        }
    }
}
