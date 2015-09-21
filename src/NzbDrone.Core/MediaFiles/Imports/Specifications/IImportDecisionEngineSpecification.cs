using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Imports.Specifications
{
    public interface IImportDecisionEngineSpecification
    {
        Decision IsSatisfiedBy(LocalItem localItem);
    }
}
