using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IImportDecisionEngineSpecification : IRejectWithReason
    {
        bool IsSatisfiedBy(LocalEpisode localEpisode);
    }
}
