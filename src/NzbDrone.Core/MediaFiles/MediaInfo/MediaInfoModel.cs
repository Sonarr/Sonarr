using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class MediaInfoModel : IEmbeddedDocument
    {
        public string RawStreamData { get; set; }

        public int SchemaRevision { get; set; }

        public string ContainerFormat { get; set; }
        public string VideoFormat { get; set; }

        public string VideoCodecID { get; set; }

        public string VideoProfile { get; set; }

        public long VideoBitrate { get; set; }

        public int VideoBitDepth { get; set; }

        public string VideoColourPrimaries { get; set; }

        public string VideoTransferCharacteristics { get; set; }

        public HdrFormat VideoHdrFormat { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public TimeSpan RunTime { get; set; }

        public decimal VideoFps { get; set; }

        public MediaInfoAudioStreamModel PrimaryAudioStream => AudioStreams?.FirstOrDefault();

        public List<MediaInfoAudioStreamModel> AudioStreams { get; set; }

        public List<MediaInfoSubtitleStreamModel> SubtitleStreams { get; set; }

        public string ScanType { get; set; }

        [JsonIgnore]
        public string Title { get; set; }
    }
}
