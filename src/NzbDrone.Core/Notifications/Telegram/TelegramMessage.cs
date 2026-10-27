using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Telegram
{
    public class TelegramMessage
    {
        public string Text { get; set; }

        /// <summary>
        /// Remote image to attach, named after the sendPhoto field it maps to.
        /// </summary>
        public string Photo { get; set; }

        public bool HasPhoto => Photo.IsNotNullOrWhiteSpace();
    }
}
