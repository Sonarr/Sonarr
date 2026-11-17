using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Telegram;

public class TelegramPhotoPayload
{
    [JsonProperty("chat_id")]
    public string ChatId { get; set; }

    [JsonProperty("parse_mode")]
    public string ParseMode { get; set; } = "HTML";

    [JsonProperty("photo")]
    public string Photo { get; set; }

    [JsonProperty("caption")]
    public string Caption { get; set; }

    [JsonProperty("disable_notification")]
    public bool DisableNotification { get; set; }

    [JsonProperty("message_thread_id")]
    public int? MessageThreadId { get; set; }
}
