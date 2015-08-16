using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Imports.Specifications.Series
{
    public abstract class BaseImportSeriesEspecification : IImportSeriesDecisionEngineSpecification
    {
        public abstract Decision IsSatisfiedBy(LocalEpisode localEpisode);

        public Decision IsSatisfiedBy(LocalItem localItem)
        {
            if (localItem is LocalEpisode)
                return IsSatisfiedBy(localItem as LocalEpisode);
            return Decision.Accept();
        }
    }
}
