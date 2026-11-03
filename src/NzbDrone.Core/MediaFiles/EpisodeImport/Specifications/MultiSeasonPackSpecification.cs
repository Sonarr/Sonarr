using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
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

            if (!isMultiSeasonPack)
            {
                return ImportSpecDecision.Accept();
            }

            if (!HasOwnSeason(localEpisode.FileEpisodeInfo, localEpisode.Series))
            {
                _logger.Debug("File in multi-season pack has no season number of its own, unable to tell which season it belongs to");

                return ImportSpecDecision.Reject(ImportRejectionReason.MultiSeason, "Unable to determine season of file in multi-season pack. Review file name or manually import");
            }

            if (IsAbsoluteOnly(localEpisode.FileEpisodeInfo))
            {
                // Packs often restart absolute numbering each season, so the folder is the only hint of which season the file belongs to
                var folderSeason = GetFolderSeason(localEpisode.Path);

                if (folderSeason.HasValue && !localEpisode.Episodes.All(e => e.SeasonNumber == folderSeason || e.SceneSeasonNumber == folderSeason))
                {
                    _logger.Debug("File in multi-season pack is in a season {0} folder, but its absolute episode number maps to a different season", folderSeason);

                    return ImportSpecDecision.Reject(ImportRejectionReason.MultiSeason, "File is in a season {0} folder, but its absolute episode number maps to a different season. Manually import", folderSeason);
                }
            }

            return ImportSpecDecision.Accept();
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

            if (IsAbsoluteOnly(fileEpisodeInfo))
            {
                return series.SeriesType == SeriesTypes.Anime;
            }

            return fileEpisodeInfo.SeasonNumbers.Any() && !fileEpisodeInfo.IsMiniSeries;
        }

        private static int? GetFolderSeason(string path)
        {
            var folder = Path.GetDirectoryName(path);

            while (folder.IsNotNullOrWhiteSpace())
            {
                var folderName = Path.GetFileName(folder);

                if (folderName.IsNotNullOrWhiteSpace())
                {
                    var season = Parser.Parser.ParseSeasonFolder(folderName);

                    if (season.HasValue)
                    {
                        return season;
                    }

                    // Stop at the pack's own folder, anything above it is unrelated to the file
                    if (Parser.Parser.ParseTitle(folderName)?.IsMultiSeason == true)
                    {
                        return null;
                    }
                }

                folder = Path.GetDirectoryName(folder);
            }

            return null;
        }

        private static bool IsAbsoluteOnly(ParsedEpisodeInfo fileEpisodeInfo)
        {
            return fileEpisodeInfo.IsAbsoluteNumbering && fileEpisodeInfo.EpisodeNumbers.Length == 0;
        }
    }
}
