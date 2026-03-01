using NzbDrone.Core.MediaFiles.MediaInfo;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.EpisodeFiles
{
    public class MediaInfoResource : RestResource
    {
        public int VideoBitDepth { get; set; }
        public long VideoBitrate { get; set; }
        public string? VideoCodec { get; set; }
        public decimal VideoFps { get; set; }
        public string? VideoDynamicRange { get; set; }
        public string? VideoDynamicRangeType { get; set; }
        public string? Resolution { get; set; }
        public string? RunTime { get; set; }
        public string? ScanType { get; set; }
        public List<MediaInfoAudioStreamResource>? AudioStreams { get; set; }
        public List<MediaInfoSubtitleStreamResource>? SubtitleStreams { get; set; }
    }

    public static class MediaInfoResourceMapper
    {
        public static MediaInfoResource? ToResource(this MediaInfoModel? model, string sceneName)
        {
            if (model == null)
            {
                return null;
            }

            return new MediaInfoResource
            {
                VideoBitDepth = model.VideoBitDepth,
                VideoBitrate = model.VideoBitrate,
                VideoCodec = MediaInfoFormatter.FormatVideoCodec(model, sceneName),
                VideoFps = Math.Round(model.VideoFps, 3),
                VideoDynamicRange = MediaInfoFormatter.FormatVideoDynamicRange(model),
                VideoDynamicRangeType = MediaInfoFormatter.FormatVideoDynamicRangeType(model),
                Resolution = $"{model.Width}x{model.Height}",
                RunTime = FormatRuntime(model.RunTime),
                ScanType = model.ScanType,
                AudioStreams = model.AudioStreams?.Select(stream => stream.ToResource(sceneName)).ToList(),
                SubtitleStreams = model.SubtitleStreams?.Select(stream => stream.ToResource()).ToList()
            };
        }

        private static string FormatRuntime(TimeSpan runTime)
        {
            var formattedRuntime = "";

            if (runTime.Hours > 0)
            {
                formattedRuntime += $"{runTime.Hours}:{runTime.Minutes:00}:";
            }
            else
            {
                formattedRuntime += $"{runTime.Minutes}:";
            }

            formattedRuntime += $"{runTime.Seconds:00}";

            return formattedRuntime;
        }
    }
}
