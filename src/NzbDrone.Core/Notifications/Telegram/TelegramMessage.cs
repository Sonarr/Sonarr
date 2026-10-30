using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Telegram
{
    public class TelegramMessage
    {
        public string Text { get; set; }

        public string Photo { get; set; }

        public bool HasPhoto => Photo.IsNotNullOrWhiteSpace();
    }
}
