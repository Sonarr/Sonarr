using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookEpisodeFileMediaInfo
    {
        public WebhookEpisodeFileMediaInfo()
        {
        }

        public WebhookEpisodeFileMediaInfo(EpisodeFile episodeFile)
        {
            AudioChannels = MediaInfoFormatter.FormatAudioChannels(episodeFile.MediaInfo);
            AudioCodec = MediaInfoFormatter.FormatAudioCodec(episodeFile.MediaInfo, episodeFile.SceneName);
            AudioLanguages = episodeFile.MediaInfo.AudioLanguages.Distinct().ToList();
            Height = episodeFile.MediaInfo.Height;
            Width = episodeFile.MediaInfo.Width;
            Subtitles = episodeFile.MediaInfo.Subtitles.Distinct().ToList();
            VideoCodec = MediaInfoFormatter.FormatVideoCodec(episodeFile.MediaInfo, episodeFile.SceneName);
            VideoDynamicRange = MediaInfoFormatter.FormatVideoDynamicRange(episodeFile.MediaInfo);
            VideoDynamicRangeType = MediaInfoFormatter.FormatVideoDynamicRangeType(episodeFile.MediaInfo);
        }

        public decimal AudioChannels { get; set; }
        public string AudioCodec { get; set; }
        public List<string> AudioLanguages { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public List<string> Subtitles { get; set; }
        public string VideoCodec { get; set; }
        public string VideoDynamicRange { get; set; }
        public string VideoDynamicRangeType { get; set; }
    }
}
