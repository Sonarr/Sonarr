using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IParsingService _parsingService;

        public MatchesFolderSpecification(IParsingService parsingService, Logger logger)
        {
            _logger = logger;
            _parsingService = parsingService;
        }
        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                return Decision.Accept();
            }

            var dirInfo = new FileInfo(localEpisode.Path).Directory;

            if (dirInfo == null)
            {
                return Decision.Accept();
            }

            var folderInfo = localEpisode.FileEpisodeInfo;

            if (folderInfo != null && folderInfo.IsPossibleSceneSeasonSpecial)
            {
                folderInfo = _parsingService.ParseSpecialEpisodeTitle(folderInfo, dirInfo.Name, localEpisode.Series.TvdbId, 0);
            }

            if (folderInfo == null)
            {
                return Decision.Accept();
            }

            if (!folderInfo.EpisodeNumbers.Any())
            {
                return Decision.Accept();
            }

            if (folderInfo.FullSeason)
            {
                return Decision.Accept();
            }

            var unexpected = localEpisode.FileEpisodeInfo.EpisodeNumbers.Where(f => !folderInfo.EpisodeNumbers.Contains(f)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected episode number(s) in file: {0}", string.Join(", ", unexpected));

                if (unexpected.Count == 1)
                {
                    return Decision.Reject("Episode Number {0} was unexpected considering the {1} folder name", unexpected.First(), dirInfo.Name);
                }

                return Decision.Reject("Episode Numbers {0} were unexpected considering the {1} folder name", string.Join(", ", unexpected), dirInfo.Name);
            }

            return Decision.Accept();
        }
    }
}
