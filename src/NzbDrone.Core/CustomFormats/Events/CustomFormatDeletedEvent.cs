using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.CustomFormats.Events
{
    public class CustomFormatDeletedEvent : IEvent
    {
        public CustomFormatDeletedEvent(CustomFormat format)
        {
            CustomFormat = format;
        }

        public CustomFormat CustomFormat { get; private set; }
    }
}
