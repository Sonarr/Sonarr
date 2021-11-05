using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateLanguage : IAggregateLocalEpisode
    {
        private readonly List<IAugmentLanguage> _augmentLanguages;
        private readonly Logger _logger;

        public AggregateLanguage(IEnumerable<IAugmentLanguage> augmentLanguages, Logger logger)
        {
            _augmentLanguages = augmentLanguages.OrderBy(a => a.Order).ToList();
            _logger = logger;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            // Get languages in preferred order, download client item, folder and finally file.
            // Non-English languages will be preferred later, in the event there is a conflict
            // between parsed languages the more preferred item will be used.

            /* var languages = new List<Language>
                             {
                                 GetLanguage(localEpisode.DownloadClientEpisodeInfo, localEpisode.Episodes),
                                 GetLanguage(localEpisode.FolderEpisodeInfo, localEpisode.Episodes),
                                 GetLanguage(localEpisode.FileEpisodeInfo, localEpisode.Episodes)
                             };*/

            var languages = new List<Language> { };
            var languagesConfidence = Confidence.Default;

            foreach (var augmentLanguage in _augmentLanguages)
            {
                var augmentedLanguage = augmentLanguage.AugmentLanguage(localEpisode, downloadClientItem);
                if (augmentedLanguage == null)
                {
                    continue;
                }

                _logger.Trace("Considering Languages {0} ({1}) from {2}", string.Join(", ", augmentedLanguage.Languages ?? new List<Language>()), augmentedLanguage.Confidence, augmentLanguage.Name);

                if (augmentedLanguage?.Languages != null && augmentedLanguage.Languages.Count > 0 && !(augmentedLanguage.Languages.Count == 1 && augmentedLanguage.Languages.Contains(Language.Unknown)))
                {
                    languages = augmentedLanguage.Languages;
                    languagesConfidence = augmentedLanguage.Confidence;
                }
            }


            var language = languages.FirstOrDefault(l => l != Language.English) ?? Language.English;

            _logger.Debug("Using language: {0}", language);

            localEpisode.Language = language;

            return localEpisode;
        }

        /*private Language GetLanguage(ParsedEpisodeInfo parsedEpisodeInfo, List<Episode> episodes)
        {
            if (parsedEpisodeInfo == null)
            {
                return Language.English;
            }

            var normalizedReleaseTitle = Parser.Parser.NormalizeEpisodeTitle(parsedEpisodeInfo.ReleaseTitle);

            foreach (var episode in episodes)
            {
                var episodeTitleLanguage = LanguageParser.ParseLanguage(episode.Title, false);

                if (episodeTitleLanguage != Language.Unknown && episodeTitleLanguage == parsedEpisodeInfo.Language)
                {
                    // Release title contains the episode title, return english instead of the parsed language.

                    if (normalizedReleaseTitle.ContainsIgnoreCase(Parser.Parser.NormalizeEpisodeTitle(episode.Title)))
                    {
                        return Language.English;
                    }
                }
            }

            return parsedEpisodeInfo.Language;
        }*/
    }
}
