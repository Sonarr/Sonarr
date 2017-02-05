using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class MediaInfoModel : IEmbeddedDocument
    {
        public string VideoCodec { get; set; }
        public int VideoBitrate { get; set; }
        public int VideoBitDepth { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string AudioFormat { get; set; }
        public int AudioBitrate { get; set; }
        public TimeSpan RunTime { get; set; }
        public int AudioStreamCount { get; set; }
        public int AudioChannels { get; set; }
        public string AudioChannelPositions { get; set; }
        public string AudioChannelPositionsText { get; set; }
        public string AudioProfile { get; set; }
        public decimal VideoFps { get; set; }
        public string AudioLanguages { get; set; }
        public string Subtitles { get; set; }
        public string ScanType { get; set; }
        public int SchemaRevision { get; set; }

        [JsonIgnore]
        public decimal FormattedAudioChannels
        {
            get
            {
                if (AudioChannelPositions.IsNullOrWhiteSpace())
                {
                    if (AudioChannelPositionsText.IsNullOrWhiteSpace())
                    {
                        if (SchemaRevision >= 3)
                        {
                            return AudioChannels;
                        }

                        return 0;
                    }

                    return AudioChannelPositionsText.ContainsIgnoreCase("LFE") ? AudioChannels - 1 + 0.1m : AudioChannels;
                }

                return AudioChannelPositions.Replace("Object Based / ", "")
                                            .Split(new string[] { " / " }, StringSplitOptions.None)
                                            .First()
                                            .Split('/')
                                            .Sum(s => decimal.Parse(s, CultureInfo.InvariantCulture));
            }
        }
    }
}
