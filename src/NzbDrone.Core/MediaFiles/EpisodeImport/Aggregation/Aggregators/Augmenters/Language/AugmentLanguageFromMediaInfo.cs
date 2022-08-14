using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromMediaInfo : IAugmentLanguage
    {
        public int Order => 4;
        public string Name => "MediaInfo";

        public AugmentLanguageResult AugmentLanguage(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.MediaInfo == null)
            {
                return null;
            }

            var audioLanguages = localEpisode.MediaInfo.AudioLanguages.Distinct().ToList();

            var languages = new List<Languages.Language>();

            foreach (var audioLanguage in audioLanguages)
            {
                var language = IsoLanguages.Find(audioLanguage)?.Language;
                languages.AddIfNotNull(language);
            }

            if (languages.Count == 0)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.MediaInfo);
        }
    }
}
