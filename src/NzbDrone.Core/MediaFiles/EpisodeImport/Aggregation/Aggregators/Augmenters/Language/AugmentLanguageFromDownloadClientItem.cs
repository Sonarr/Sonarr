using System.Collections.Generic;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromDownloadClientItem : IAugmentLanguage
    {
        public int Order => 3;
        public string Name => "DownloadClientItem";

        public AugmentLanguageResult AugmentLanguage(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var languages = new List<Languages.Language> { localEpisode.DownloadClientEpisodeInfo?.Language };

            if (languages == null)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.DownloadClientItem);
        }
    }
}
