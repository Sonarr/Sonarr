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
        public string ContainerFormat { get; set; }

        // Deprecated according to MediaInfo
        public string VideoCodec { get; set; }
        public string VideoFormat { get; set; }
        public string VideoCodecID { get; set; }
        public string VideoProfile { get; set; }
        public string VideoCodecLibrary { get; set; }
        public int VideoBitrate { get; set; }
        public int VideoBitDepth { get; set; }
        public int VideoMultiViewCount { get; set; }
        public string VideoColourPrimaries { get; set; }
        public string VideoTransferCharacteristics { get; set; }
        public string VideoHdrFormat { get; set; }
        public string VideoHdrFormatCompatibility { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string AudioFormat { get; set; }
        public string AudioCodecID { get; set; }
        public string AudioCodecLibrary { get; set; }
        public string AudioAdditionalFeatures { get; set; }
        public int AudioBitrate { get; set; }
        public TimeSpan RunTime { get; set; }
        public int AudioStreamCount { get; set; }
        public int AudioChannelsContainer { get; set; }
        public int AudioChannelsStream { get; set; }
        public string AudioChannelPositions { get; set; }
        public string AudioChannelPositionsTextContainer { get; set; }
        public string AudioChannelPositionsTextStream { get; set; }
        public string AudioProfile { get; set; }
        public decimal VideoFps { get; set; }
        public string AudioLanguages { get; set; }
        public string Subtitles { get; set; }
        public string ScanType { get; set; }
        public int SchemaRevision { get; set; }
    }
}
