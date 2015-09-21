using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Imports.Specifications
{
    public interface IImportMovieDecisionEngineSpecification : IImportDecisionEngineSpecification
    {
        Decision IsSatisfiedBy(LocalMovie localMovie);
    }
}
