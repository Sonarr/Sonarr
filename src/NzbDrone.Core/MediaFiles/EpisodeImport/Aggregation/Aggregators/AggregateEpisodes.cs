using System.IO;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateEpisodes : IAggregateLocalEpisode
    {
        private readonly IParsingService _parsingService;

        public AggregateEpisodes(IParsingService parsingService)
        {
            _parsingService = parsingService;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, bool otherFiles)
        {
            var bestEpisodeInfoForEpisodes = GetBestEpisodeInfo(localEpisode, otherFiles);

            localEpisode.Episodes = _parsingService.GetEpisodes(bestEpisodeInfoForEpisodes, localEpisode.Series, localEpisode.SceneSource);

            return localEpisode;
        }

        private ParsedEpisodeInfo GetBestEpisodeInfo(LocalEpisode localEpisode, bool otherFiles)
        {
            var parsedEpisodeInfo = localEpisode.FileEpisodeInfo;
            var downloadClientEpisodeInfo = localEpisode.DownloadClientEpisodeInfo;
            var folderEpisodeInfo = localEpisode.FolderEpisodeInfo;

            if (!otherFiles && !SceneChecker.IsSceneTitle(Path.GetFileNameWithoutExtension(localEpisode.Path)))
            {
                if (downloadClientEpisodeInfo != null && !downloadClientEpisodeInfo.FullSeason)
                {
                    parsedEpisodeInfo = localEpisode.DownloadClientEpisodeInfo;
                }
                else if (folderEpisodeInfo != null && !folderEpisodeInfo.FullSeason)
                {
                    parsedEpisodeInfo = localEpisode.FolderEpisodeInfo;
                }
            }

            if (parsedEpisodeInfo == null || parsedEpisodeInfo.IsPossibleSpecialEpisode)
            {
                var title = Path.GetFileNameWithoutExtension(localEpisode.Path);
                var specialEpisodeInfo = _parsingService.ParseSpecialEpisodeTitle(parsedEpisodeInfo, title, localEpisode.Series);

                return specialEpisodeInfo;
            }

            return parsedEpisodeInfo;
        }
    }
}
