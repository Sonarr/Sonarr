using System;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class EpisodeTitleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IBuildFileNames _buildFileNames;
        private readonly Logger _logger;

        public EpisodeTitleSpecification(IBuildFileNames buildFileNames, Logger logger)
        {
            _buildFileNames = buildFileNames;
            _logger = logger;
        }
        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (!_buildFileNames.RequiresEpisodeTitle(localEpisode.Series, localEpisode.Episodes))
            {
                _logger.Debug("File name format does not require episode title, skipping check");
                return Decision.Accept();
            }

            foreach (var episode in localEpisode.Episodes)
            {
                var airDateUtc = episode.AirDateUtc;
                var title = episode.Title;

                if (airDateUtc.HasValue && airDateUtc.Value.Before(DateTime.UtcNow.AddDays(-1)))
                {
                    _logger.Debug("Episode aired more than 1 day ago");
                    continue;
                }

                if (title.IsNullOrWhiteSpace())
                {
                    _logger.Debug("Episode does not have a title and recently aired");

                    return Decision.Reject("Episode does not have a title and recently aired");
                }

                if (title.Equals("TBA"))
                {
                    _logger.Debug("Episode has a TBA title and recently aired");

                    return Decision.Reject("Episode has a TBA title and recently aired");
                }
            }

            return Decision.Accept();
        }
    }
}
