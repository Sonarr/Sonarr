using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesFolderSpecification(Logger logger)
        {
            _logger = logger;
        }
        public Decision IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.ExistingFile)
            {
                return Decision.Accept();
            }

            var folderInfo = Parser.Parser.ParseTitle(new FileInfo(localEpisode.Path).DirectoryName);

            if (folderInfo == null)
            {
                return Decision.Accept();
            }

            if (folderInfo.FullSeason)
            {
                return Decision.Accept();
            }

            var unexpected = localEpisode.ParsedEpisodeInfo.EpisodeNumbers.Where(f => !folderInfo.EpisodeNumbers.Contains(f)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected episode number(s) in file: {0}", unexpected);

                if (unexpected.Count == 1)
                {
                    return Decision.Reject("Episode Number {0} was unexpected", unexpected.First());
                }

                return Decision.Reject("Episode Numbers {0} were unexpected", String.Join(", ", unexpected));
            }

            return Decision.Accept();
        }
    }
}
