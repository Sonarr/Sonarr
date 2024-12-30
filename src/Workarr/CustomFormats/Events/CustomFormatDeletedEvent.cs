using Workarr.Messaging;

namespace Workarr.CustomFormats.Events
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
