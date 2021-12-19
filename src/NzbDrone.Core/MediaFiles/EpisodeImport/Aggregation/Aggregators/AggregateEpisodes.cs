using System.Collections.Generic;
using System.IO;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateEpisodes : IAggregateLocalEpisode
    {
        private readonly IParsingService _parsingService;

        public AggregateEpisodes(IParsingService parsingService)
        {
            _parsingService = parsingService;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            localEpisode.Episodes = GetEpisodes(localEpisode);

            return localEpisode;
        }

        private ParsedEpisodeInfo GetBestEpisodeInfo(LocalEpisode localEpisode)
        {
            var parsedEpisodeInfo = localEpisode.FileEpisodeInfo;
            var downloadClientEpisodeInfo = localEpisode.DownloadClientEpisodeInfo;
            var folderEpisodeInfo = localEpisode.FolderEpisodeInfo;

            if (!localEpisode.OtherVideoFiles && !SceneChecker.IsSceneTitle(Path.GetFileNameWithoutExtension(localEpisode.Path)))
            {
                if (downloadClientEpisodeInfo != null &&
                    !downloadClientEpisodeInfo.FullSeason &&
                    PreferOtherEpisodeInfo(parsedEpisodeInfo, downloadClientEpisodeInfo))
                {
                    parsedEpisodeInfo = localEpisode.DownloadClientEpisodeInfo;
                }
                else if (folderEpisodeInfo != null &&
                         !folderEpisodeInfo.FullSeason &&
                         PreferOtherEpisodeInfo(parsedEpisodeInfo, folderEpisodeInfo))
                {
                    parsedEpisodeInfo = localEpisode.FolderEpisodeInfo;
                }
            }

            if (parsedEpisodeInfo == null)
            {
                parsedEpisodeInfo = GetSpecialEpisodeInfo(localEpisode, parsedEpisodeInfo);
            }

            return parsedEpisodeInfo;
        }

        private ParsedEpisodeInfo GetSpecialEpisodeInfo(LocalEpisode localEpisode, ParsedEpisodeInfo parsedEpisodeInfo)
        {
            var title = Path.GetFileNameWithoutExtension(localEpisode.Path);
            var specialEpisodeInfo = _parsingService.ParseSpecialEpisodeTitle(parsedEpisodeInfo, title, localEpisode.Series);

            return specialEpisodeInfo;
        }

        private List<Episode> GetEpisodes(LocalEpisode localEpisode)
        {
            var bestEpisodeInfoForEpisodes = GetBestEpisodeInfo(localEpisode);
            var isMediaFile = MediaFileExtensions.Extensions.Contains(Path.GetExtension(localEpisode.Path));

            if (bestEpisodeInfoForEpisodes == null)
            {
                return new List<Episode>();
            }

            if (ValidateParsedEpisodeInfo.ValidateForSeriesType(bestEpisodeInfoForEpisodes, localEpisode.Series, isMediaFile))
            {
                var episodes = _parsingService.GetEpisodes(bestEpisodeInfoForEpisodes, localEpisode.Series, localEpisode.SceneSource);

                if (episodes.Empty() && bestEpisodeInfoForEpisodes.IsPossibleSpecialEpisode)
                {
                    var parsedSpecialEpisodeInfo = GetSpecialEpisodeInfo(localEpisode, bestEpisodeInfoForEpisodes);

                    if (parsedSpecialEpisodeInfo != null)
                    {
                        episodes = _parsingService.GetEpisodes(parsedSpecialEpisodeInfo, localEpisode.Series, localEpisode.SceneSource);
                    }
                }

                return episodes;
            }

            return new List<Episode>();
        }

        private bool PreferOtherEpisodeInfo(ParsedEpisodeInfo fileEpisodeInfo, ParsedEpisodeInfo otherEpisodeInfo)
        {
            if (fileEpisodeInfo == null)
            {
                return true;
            }

            // When the files episode info is not absolute prefer it over a parsed episode info that is absolute
            if (!fileEpisodeInfo.IsAbsoluteNumbering && otherEpisodeInfo.IsAbsoluteNumbering)
            {
                return false;
            }

            return true;
        }
    }
}
