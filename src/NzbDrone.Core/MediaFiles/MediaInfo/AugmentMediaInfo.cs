using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public interface IAugmentMediaInfo
    {
        LocalEpisode Augment(LocalEpisode localEpisode);
    }

    public class AugmentMediaInfo
    {
        private readonly IConfigService _configService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;

        public AugmentMediaInfo(IConfigService configService, IVideoFileInfoReader videoFileInfoReader)
        {
            _configService = configService;
            _videoFileInfoReader = videoFileInfoReader;
        }

        public LocalEpisode Augment(LocalEpisode localEpisode)
        {
            if (!localEpisode.ExistingFile || _configService.EnableMediaInfo)
            {
                localEpisode.MediaInfo = _videoFileInfoReader.GetMediaInfo(localEpisode.Path);
            }

            return localEpisode;
        }
    }
}
