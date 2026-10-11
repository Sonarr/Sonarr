using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class MultiSeasonPackSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MultiSeasonPackSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var isMultiSeasonPack = (localEpisode.DownloadClientEpisodeInfo?.IsMultiSeason ?? false) ||
                                    (localEpisode.FolderEpisodeInfo?.IsMultiSeason ?? false);

            if (!isMultiSeasonPack || HasOwnSeason(localEpisode.FileEpisodeInfo, localEpisode.Series))
            {
                return ImportSpecDecision.Accept();
            }

            _logger.Debug("File in multi-season pack has no season number of its own, unable to tell which season it belongs to");

            return ImportSpecDecision.Reject(ImportRejectionReason.MultiSeason, "Unable to determine season of file in multi-season pack. Review file name or manually import");
        }

        private static bool HasOwnSeason(ParsedEpisodeInfo fileEpisodeInfo, Series series)
        {
            if (fileEpisodeInfo == null)
            {
                return false;
            }

            if (fileEpisodeInfo.IsDaily)
            {
                return true;
            }

            if (fileEpisodeInfo.IsAbsoluteNumbering && fileEpisodeInfo.EpisodeNumbers.Length == 0)
            {
                return series.SeriesType == SeriesTypes.Anime;
            }

            return fileEpisodeInfo.SeasonNumbers.Any() && !fileEpisodeInfo.IsMiniSeries;
        }
    }
}
