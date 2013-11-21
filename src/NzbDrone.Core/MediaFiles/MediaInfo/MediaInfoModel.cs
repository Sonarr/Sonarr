using System;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class MediaInfoModel
    {
        public string VideoCodec { get; set; }
        public int VideoBitrate { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string AudioFormat { get; set; }
        public int AudioBitrate { get; set; }
        public TimeSpan RunTime { get; set; }
        public int AudioStreamCount { get; set; }
        public int AudioChannels { get; set; }
        public string AudioProfile { get; set; }
        public decimal VideoFps { get; set; }
        public string AudioLanguages { get; set; }
        public string Subtitles { get; set; }
        public string ScanType { get; set; }
    }
}
