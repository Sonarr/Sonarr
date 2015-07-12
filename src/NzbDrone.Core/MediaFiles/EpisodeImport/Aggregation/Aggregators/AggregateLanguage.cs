using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateLanguage : IAggregateLocalEpisode
    {
        private readonly Logger _logger;

        public AggregateLanguage(Logger logger)
        {
            _logger = logger;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, bool otherFiles)
        {
            // Get languages in preferred order, download client item, folder and finally file.
            // Non-English languages will be preferred later, in the event there is a conflict
            // between parsed languages the more preferred item will be used.

            var languages = new List<Language>
                            {
                                GetLanguage(localEpisode.DownloadClientEpisodeInfo),
                                GetLanguage(localEpisode.FolderEpisodeInfo),
                                GetLanguage(localEpisode.FileEpisodeInfo)
                            };

            var language = languages.FirstOrDefault(l => l != Language.English) ?? Language.English;

            _logger.Debug("Using language: {0}", language);

            localEpisode.Language = language;

            return localEpisode;
        }

        private Language GetLanguage(ParsedEpisodeInfo parsedEpisodeInfo)
        {
            if (parsedEpisodeInfo == null)
            {
                // English is the default language when otherwise unknown

                return Language.English;
            }

            return parsedEpisodeInfo.Language;
        }
    }
}
