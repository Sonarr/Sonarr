using System.Linq;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification upgradableSpecification,
                                        ICustomFormatCalculationService formatService,
                                        Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                if (file == null)
                {
                    _logger.Debug("File is no longer available, skipping this file.");
                    continue;
                }

                var customFormats = _formatService.ParseCustomFormat(file);

                _logger.Debug("Comparing file quality and language with report. Existing file is {0} - {1}", file.Quality, file.Language);

                if (!_upgradableSpecification.IsUpgradable(subject.Series.QualityProfile,
                                                           subject.Series.LanguageProfile,
                                                           file.Quality,
                                                           file.Language,
                                                           customFormats,
                                                           subject.ParsedEpisodeInfo.Quality,
                                                           subject.ParsedEpisodeInfo.Language,
                                                           subject.CustomFormats))
                {
                    return Decision.Reject("Existing file on disk is of equal or higher preference: {0} - {1}", file.Quality, file.Language);
                }
            }

            return Decision.Accept();
        }
    }
}
