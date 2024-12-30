using Workarr.Download;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport
{
    public interface IImportDecisionEngineSpecification
    {
        ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem);
    }
}
