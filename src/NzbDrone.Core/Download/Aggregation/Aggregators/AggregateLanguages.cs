using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public class AggregateLanguages : IAggregateRemoteEpisode
    {
        private readonly Logger _logger;

        public AggregateLanguages(Logger logger)
        {
            _logger = logger;
        }

        public RemoteEpisode Aggregate(RemoteEpisode remoteEpisode)
        {
            var parsedEpisodeInfo = remoteEpisode.ParsedEpisodeInfo;
            var releaseInfo = remoteEpisode.Release;
            var languages = parsedEpisodeInfo.Languages;
            var series = remoteEpisode.Series;
            var releaseTokens = parsedEpisodeInfo.ReleaseTokens ?? parsedEpisodeInfo.ReleaseTitle;
            var normalizedReleaseTokens = Parser.Parser.NormalizeEpisodeTitle(releaseTokens);
            var languagesToRemove = new List<Language>();

            if (series == null)
            {
                _logger.Debug("Unable to aggregate languages, using parsed values: {0}", string.Join(", ", languages.ToList()));

                remoteEpisode.Languages = languages;
                remoteEpisode.Languages = releaseInfo != null && releaseInfo.Languages.Any() ? releaseInfo.Languages : languages;

                return remoteEpisode;
            }

            if (releaseInfo != null && releaseInfo.Languages.Any())
            {
                _logger.Debug("Languages provided by indexer, using release values: {0}", string.Join(", ", releaseInfo.Languages));

                // Use languages from release (given by indexer or user) if available
                languages = releaseInfo.Languages;
            }
            else
            {
                // Exclude any languages that are part of the episode title, if the episode title is in the release tokens (falls back to release title)
                foreach (var episode in remoteEpisode.Episodes)
                {
                    var episodeTitleLanguage = LanguageParser.ParseLanguages(episode.Title);

                    if (!episodeTitleLanguage.Contains(Language.Unknown))
                    {
                        var normalizedEpisodeTitle = Parser.Parser.NormalizeEpisodeTitle(episode.Title);
                        var episodeTitleIndex = normalizedReleaseTokens.IndexOf(normalizedEpisodeTitle,
                            StringComparison.CurrentCultureIgnoreCase);

                        if (episodeTitleIndex >= 0)
                        {
                            releaseTokens = releaseTokens.Remove(episodeTitleIndex, normalizedEpisodeTitle.Length);
                            languagesToRemove.AddRange(episodeTitleLanguage);
                        }
                    }
                }

                // Remove any languages still in the title that would normally be removed
                languagesToRemove = languagesToRemove.Except(LanguageParser.ParseLanguages(releaseTokens)).ToList();

                // Remove all languages that aren't part of the updated releaseTokens
                languages = languages.Except(languagesToRemove).ToList();
            }

            // Use series language as fallback if we couldn't parse a language
            if (languages.Count == 0 || (languages.Count == 1 && languages.First() == Language.Unknown))
            {
                languages = new List<Language> { series.OriginalLanguage };
                _logger.Debug("Language couldn't be parsed from release, fallback to series original language: {0}", series.OriginalLanguage.Name);
            }

            if (languages.Contains(Language.Original))
            {
                languages.Remove(Language.Original);

                if (!languages.Contains(series.OriginalLanguage))
                {
                    languages.Add(series.OriginalLanguage);
                }
                else
                {
                    languages.Add(Language.Unknown);
                }
            }

            _logger.Debug("Selected languages: {0}", string.Join(", ", languages.ToList()));

            remoteEpisode.Languages = languages;

            return remoteEpisode;
        }
    }
}
