using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaFiles.MediaInfo;

public class MediaInfoAudioStreamModel : IEmbeddedDocument
{
    public string Language { get; set; }
    public string Format { get; set; }
    public string CodecId { get; set; }
    public string Profile { get; set; }
    public long Bitrate { get; set; }
    public int Channels { get; set; }
    public string ChannelPositions { get; set; }
    public string Title { get; set; }
}
