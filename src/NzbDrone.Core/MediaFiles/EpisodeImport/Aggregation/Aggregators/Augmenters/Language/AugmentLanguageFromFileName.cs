using System.Collections.Generic;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromFileName : IAugmentLanguage
    {
        public int Order => 1;
        public string Name => "FileName";

        public AugmentLanguageResult AugmentLanguage(LocalEpisode localMovie, DownloadClientItem downloadClientItem)
        {
            var languages = new List<Languages.Language> { localMovie.FileEpisodeInfo?.Language };

            if (languages == null)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.Filename);
        }
    }
}
