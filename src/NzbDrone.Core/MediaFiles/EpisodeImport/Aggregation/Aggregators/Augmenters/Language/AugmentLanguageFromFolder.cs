using System.Collections.Generic;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromFolder : IAugmentLanguage
    {
        public int Order => 2;
        public string Name => "FolderName";

        public AugmentLanguageResult AugmentLanguage(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var languages = new List<Languages.Language> { localEpisode.FolderEpisodeInfo?.Language };

            if (languages == null)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.Foldername);
        }
    }
}
