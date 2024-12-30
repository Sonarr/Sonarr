using Workarr.Download;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public interface IAugmentLanguage
    {
        int Order { get; }
        string Name { get; }
        AugmentLanguageResult AugmentLanguage(LocalEpisode localEpisode, DownloadClientItem downloadClientItem);
    }
}
