using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Telegram
{
    public enum TelegramImageType
    {
        [FieldOption(Label = "None")]
        None = 0,

        [FieldOption(Label = "Episode Screenshot")]
        Screenshot = 1,

        [FieldOption(Label = "Fanart")]
        Fanart = 2,

        [FieldOption(Label = "Poster")]
        Poster = 3
    }
}
