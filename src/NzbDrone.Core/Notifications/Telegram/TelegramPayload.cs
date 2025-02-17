using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Telegram;

public class TelegramPayload
{
    [JsonProperty("chat_id")]
    public string ChatId { get; set; }

    [JsonProperty("parse_mode")]
    public string ParseMode = "HTML";
    public string Text { get; set; }

    [JsonProperty("disable_notification")]
    public bool DisableNotification { get; set; }

    [JsonProperty("message_thread_id")]
    public int? MessageThreadId { get; set; }

    [JsonProperty("link_preview_options")]
    public TelegramLinkPreviewOptions LinkPreviewOptions { get; set; }
}
