using NzbDrone.Core.MediaFiles.MediaInfo;

namespace Sonarr.Api.V5.EpisodeFiles;

public abstract class MediaInfoStreamResource
{
    public string? Language { get; set; }
}

public sealed class MediaInfoAudioStreamResource : MediaInfoStreamResource
{
    public string? Codec { get; set; }
    public string? CodecId { get; set; }
    public string? Profile { get; set; }
    public long Bitrate { get; set; }
    public decimal Channels { get; set; }
    public string? ChannelPositions { get; set; }
    public string? Title { get; set; }
}

public sealed class MediaInfoSubtitleStreamResource : MediaInfoStreamResource
{
    public string? Format { get; set; }
    public string? Title { get; set; }
    public bool? Forced { get; set; }
    public bool? HearingImpaired { get; set; }
}

public static class MediaInfoAudioStreamResourceMapper
{
    public static MediaInfoAudioStreamResource ToResource(this MediaInfoAudioStreamModel model, string sceneName)
    {
        return new MediaInfoAudioStreamResource
        {
            Language = model.Language,
            Codec = MediaInfoFormatter.FormatAudioCodec(model, sceneName),
            CodecId = model.CodecId,
            Profile = model.Profile,
            Bitrate = model.Bitrate,
            Channels = MediaInfoFormatter.FormatAudioChannels(model),
            ChannelPositions = model.ChannelPositions,
            Title = model.Title,
        };
    }
}

public static class MediaInfoSubtitleStreamResourceMapper
{
    public static MediaInfoSubtitleStreamResource ToResource(this MediaInfoSubtitleStreamModel model)
    {
        return new MediaInfoSubtitleStreamResource
        {
            Language = model.Language,
            Format = model.Format,
            Title = model.Title,
            Forced = model.Forced,
            HearingImpaired = model.HearingImpaired,
        };
    }
}
