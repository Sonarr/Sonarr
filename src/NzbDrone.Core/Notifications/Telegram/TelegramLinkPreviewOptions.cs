using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Telegram;

public class TelegramLinkPreviewOptions
{
    [JsonProperty("is_disabled")]
    public bool IsDisabled { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    public TelegramLinkPreviewOptions(List<NotificationMetadataLink> links, TelegramSettings settings)
    {
        IsDisabled = (MetadataLinkPreviewType)settings.LinkPreview == MetadataLinkPreviewType.None;
        Url = links.FirstOrDefault(l => l.Type.HasValue && (int)l.Type.Value == settings.LinkPreview)?.Link;
    }
}
