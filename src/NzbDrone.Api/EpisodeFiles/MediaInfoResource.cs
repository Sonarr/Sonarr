using NzbDrone.Core.MediaFiles.MediaInfo;
using Sonarr.Http.REST;

namespace NzbDrone.Api.EpisodeFiles
{
    public class MediaInfoResource : RestResource
    {
        public decimal AudioChannels { get; set; }
        public string AudioCodec { get; set; }
        public string VideoCodec { get; set; }
    }

    public static class MediaInfoResourceMapper
    {
        public static MediaInfoResource ToResource(this MediaInfoModel model, string sceneName)
        {
            if (model == null)
            {
                return null;
            }

            return new MediaInfoResource
                   {
                       AudioChannels = MediaInfoFormatter.FormatAudioChannels(model),
                       AudioCodec = MediaInfoFormatter.FormatAudioCodec(model, sceneName),
                       VideoCodec = MediaInfoFormatter.FormatVideoCodec(model, sceneName)
                   };
        }
    }
}
