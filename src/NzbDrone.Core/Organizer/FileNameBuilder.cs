using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile);
        string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig namingConfig);
        string BuildFilePath(Series series, int seasonNumber, string fileName, string extension);
        BasicNamingConfig GetBasicNamingConfig(NamingConfig nameSpec);
        string GetSeriesFolder(string seriesTitle);
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly ICached<EpisodeFormat> _patternCache;
        private readonly Logger _logger;

        private static readonly Regex TitleRegex = new Regex(@"(?<token>\{(?:\w+)(?<separator>\s|\.|-|_)\w+\})",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EpisodeRegex = new Regex(@"(?<episode>\{episode(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SeasonRegex = new Regex(@"(?<season>\{season(?:\:0+)?})",
                                                              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SeasonEpisodePatternRegex = new Regex(@"(?<separator>(?<=}).+?)?(?<seasonEpisode>s?{season(?:\:0+)?}(?<episodeSeparator>e|x)(?<episode>{episode(?:\:0+)?}))(?<separator>.+?(?={))?",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AirDateRegex = new Regex(@"\{Air(\s|\W|_)Date\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SeriesTitleRegex = new Regex(@"(?<token>\{(?:Series)(?<separator>\s|\.|-|_)Title\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FileNameBuilder(INamingConfigService namingConfigService,
                               ICacheManger cacheManger,
                               Logger logger)
        {
            _namingConfigService = namingConfigService;
            _patternCache = cacheManger.GetCache<EpisodeFormat>(GetType());
            _logger = logger;
        }

        public string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile)
        {
            var nameSpec = _namingConfigService.GetConfig();

            return BuildFilename(episodes, series, episodeFile, nameSpec);
        }

        public string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig namingConfig)
        {
            if (!namingConfig.RenameEpisodes)
            {
                if (String.IsNullOrWhiteSpace(episodeFile.SceneName))
                {
                    return Path.GetFileNameWithoutExtension(episodeFile.Path);
                }

                return episodeFile.SceneName;
            }

            if (String.IsNullOrWhiteSpace(namingConfig.StandardEpisodeFormat) && series.SeriesType == SeriesTypes.Standard)
            {
                throw new NamingFormatException("Standard episode format cannot be null");
            }

            if (String.IsNullOrWhiteSpace(namingConfig.DailyEpisodeFormat) && series.SeriesType == SeriesTypes.Daily)
            {
                throw new NamingFormatException("Daily episode format cannot be null");
            }

            var sortedEpisodes = episodes.OrderBy(e => e.EpisodeNumber).ToList();
            var pattern = namingConfig.StandardEpisodeFormat;
            var episodeTitles = new List<string>
            {
                sortedEpisodes.First().Title
            };

            var tokenValues = new Dictionary<string, string>(FilenameBuilderTokenEqualityComparer.Instance)
            {
                {"{Series Title}", series.Title},
                {"Original Title", episodeFile.SceneName}
            };

            tokenValues.Add("{Release Group}", episodeFile.ReleaseGroup);

            if (series.SeriesType == SeriesTypes.Daily)
            {
                pattern = namingConfig.DailyEpisodeFormat;

                if (!String.IsNullOrWhiteSpace(episodes.First().AirDate))
                {
                    tokenValues.Add("{Air Date}", episodes.First().AirDate.Replace('-', ' '));
                }

                else {
                    tokenValues.Add("{Air Date}", "Unknown");
                }
            }

            var episodeFormat = GetEpisodeFormat(pattern);

            if (episodeFormat != null)
            {
                pattern = pattern.Replace(episodeFormat.SeasonEpisodePattern, "{Season Episode}");
                var seasonEpisodePattern = episodeFormat.SeasonEpisodePattern;

                foreach (var episode in sortedEpisodes.Skip(1))
                {
                    switch ((MultiEpisodeStyle)namingConfig.MultiEpisodeStyle)
                    {
                        case MultiEpisodeStyle.Duplicate:
                            seasonEpisodePattern += episodeFormat.Separator + episodeFormat.SeasonEpisodePattern;
                            break;

                        case MultiEpisodeStyle.Repeat:
                            seasonEpisodePattern += episodeFormat.EpisodeSeparator + episodeFormat.EpisodePattern;
                            break;

                        case MultiEpisodeStyle.Scene:
                            seasonEpisodePattern += "-" + episodeFormat.EpisodeSeparator + episodeFormat.EpisodePattern;
                            break;

                        //MultiEpisodeStyle.Extend
                        default:
                            seasonEpisodePattern += "-" + episodeFormat.EpisodePattern;
                            break;
                    }

                    episodeTitles.Add(episode.Title);
                }

                seasonEpisodePattern = ReplaceNumberTokens(seasonEpisodePattern, sortedEpisodes);
                tokenValues.Add("{Season Episode}", seasonEpisodePattern);
            }

            tokenValues.Add("{Episode Title}", GetEpisodeTitle(episodeTitles));
            tokenValues.Add("{Quality Title}", episodeFile.Quality.ToString());
            

            return CleanFilename(ReplaceTokens(pattern, tokenValues).Trim());
        }

        public string BuildFilePath(Series series, int seasonNumber, string fileName, string extension)
        {
            string path = series.Path;

            if (series.SeasonFolder)
            {
                string seasonFolder;

                if (seasonNumber == 0)
                {
                    seasonFolder = "Specials";
                }

                else
                {
                    var nameSpec = _namingConfigService.GetConfig();
                    var tokenValues = new Dictionary<string, string>(FilenameBuilderTokenEqualityComparer.Instance);
                    tokenValues.Add("{Series Title}", series.Title);
                    
                    seasonFolder = ReplaceSeasonTokens(nameSpec.SeasonFolderFormat, seasonNumber);
                    seasonFolder = ReplaceTokens(seasonFolder, tokenValues);
                }

                path = Path.Combine(path, seasonFolder);
            }

            return Path.Combine(path, fileName + extension);
        }

        public BasicNamingConfig GetBasicNamingConfig(NamingConfig nameSpec)
        {
            var episodeFormat = GetEpisodeFormat(nameSpec.StandardEpisodeFormat);

            if (episodeFormat == null)
            {
                return new BasicNamingConfig();
            }

            var basicNamingConfig = new BasicNamingConfig
                                    {
                                        Separator = episodeFormat.Separator,
                                        NumberStyle = episodeFormat.SeasonEpisodePattern
                                    };

            var titleTokens = TitleRegex.Matches(nameSpec.StandardEpisodeFormat);

            foreach (Match match in titleTokens)
            {
                var separator = match.Groups["separator"].Value;
                var token = match.Groups["token"].Value;

                if (!separator.Equals(" "))
                {
                    basicNamingConfig.ReplaceSpaces = true;
                }

                if (token.StartsWith("{Series", StringComparison.InvariantCultureIgnoreCase))
                {
                    basicNamingConfig.IncludeSeriesTitle = true;
                }

                if (token.StartsWith("{Episode", StringComparison.InvariantCultureIgnoreCase))
                {
                    basicNamingConfig.IncludeEpisodeTitle = true;
                }

                if (token.StartsWith("{Quality", StringComparison.InvariantCultureIgnoreCase))
                {
                    basicNamingConfig.IncludeQuality = true;
                }
            }

            return basicNamingConfig;
        }

        public string GetSeriesFolder(string seriesTitle)
        {
            seriesTitle = CleanFilename(seriesTitle);

            var nameSpec = _namingConfigService.GetConfig();
            var tokenValues = new Dictionary<string, string>(FilenameBuilderTokenEqualityComparer.Instance);
            tokenValues.Add("{Series Title}", seriesTitle);

            return ReplaceTokens(nameSpec.SeriesFolderFormat, tokenValues);
        }

        public static string CleanFilename(string name)
        {
            string result = name;
            string[] badCharacters = { "\\", "/", "<", ">", "?", "*", ":", "|", "\"" };
            string[] goodCharacters = { "+", "+", "{", "}", "!", "@", "-", "#", "`" };

            for (int i = 0; i < badCharacters.Length; i++)
                result = result.Replace(badCharacters[i], goodCharacters[i]);

            return result.Trim();
        }

        private string ReplaceTokens(string pattern, Dictionary<string, string> tokenValues)
        {
            return TitleRegex.Replace(pattern, match => ReplaceToken(match, tokenValues));
        }

        private string ReplaceToken(Match match, Dictionary<string, string> tokenValues)
        {
            var separator = match.Groups["separator"].Value;
            var token = match.Groups["token"].Value;
            var replacementText = "";
            var patternTokenArray = token.ToCharArray();
            if (!tokenValues.TryGetValue(token, out replacementText)) return null;

            if (patternTokenArray.All(t => !Char.IsLetter(t) || Char.IsLower(t)))
            {
                replacementText = replacementText.ToLowerInvariant();
            }

            else if (patternTokenArray.All(t => !Char.IsLetter(t) || Char.IsUpper(t)))
            {
                replacementText = replacementText.ToUpper();
            }

            if (!separator.Equals(" "))
            {
                replacementText = replacementText.Replace(" ", separator);
            }

            return replacementText;
        }

        private string ReplaceNumberTokens(string pattern, List<Episode> episodes)
        {
            var episodeIndex = 0;
            pattern = EpisodeRegex.Replace(pattern, match =>
            {
                var episode = episodes[episodeIndex].EpisodeNumber;
                episodeIndex++;

                return ReplaceNumberToken(match.Groups["episode"].Value, episode);
            });

            return ReplaceSeasonTokens(pattern, episodes.First().SeasonNumber);
        }

        private string ReplaceSeasonTokens(string pattern, int seasonNumber)
        {
            return SeasonRegex.Replace(pattern, match => ReplaceNumberToken(match.Groups["season"].Value, seasonNumber));
        }

        private string ReplaceNumberToken(string token, int value)
        {
            var split = token.Trim('{', '}').Split(':');
            if (split.Length == 1) return value.ToString("0");

            return value.ToString(split[1]);
        }

        private EpisodeFormat GetEpisodeFormat(string pattern)
        {
            return _patternCache.Get(pattern, () =>
            {
                var match = SeasonEpisodePatternRegex.Match(pattern);

                if (match.Success)
                {
                    return new EpisodeFormat
                    {
                        EpisodeSeparator = match.Groups["episodeSeparator"].Value,
                        Separator = match.Groups["separator"].Value,
                        EpisodePattern = match.Groups["episode"].Value,
                        SeasonEpisodePattern = match.Groups["seasonEpisode"].Value,
                    };

                }

                return null;
            });
        }

        private string GetEpisodeTitle(List<string> episodeTitles)
        {
            if (episodeTitles.Count == 1)
            {
                return episodeTitles.First();
            }

            return String.Join(" + ", episodeTitles.Select(Parser.Parser.CleanupEpisodeTitle).Distinct());
        }
    }

    public enum MultiEpisodeStyle
    {
        Extend = 0,
        Duplicate = 1,
        Repeat = 2,
        Scene = 3
    }
}