using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.Imports.Specifications.Common
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UpgradeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalItem localItem)
        {
            var qualityComparer = new QualityModelComparer(localItem.Media.Profile);
            foreach (var file in localItem.MediaFiles)
            {
                if (qualityComparer.Compare(file.Quality, localItem.Quality) > 0)
                {
                    _logger.Debug("This file isn't an upgrade for all files. Skipping {0}", localItem.Path);
                    return Decision.Reject("Not an upgrade for existing episode file(s)");
                }
            }
            return Decision.Accept();
        }
    }
}
