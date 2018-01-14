using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IImportDecisionEngineSpecification
    {
        Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem);
    }
}
