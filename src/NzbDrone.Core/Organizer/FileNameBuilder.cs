using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile);
        string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig namingConfig);
        string BuildFilePath(Series series, int seasonNumber, string fileName, string extension);
    }



    public class FileNameBuilder : IBuildFileNames
    {
        private readonly IConfigService _configService;
        private readonly INamingConfigService _namingConfigService;
        private readonly Logger _logger;

        private static readonly Regex TitleRegex = new Regex(@"(?<token>\{(?:\w+)(?<separator>\s|\W|_)\w+\})",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EpisodeRegex = new Regex(@"(?<episode>\{episode(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SeasonRegex = new Regex(@"(?<season>\{season(?:\:0+)?})",
                                                              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SeasonEpisodePatternRegex = new Regex(@"(?<separator>(?<=}).+?)?(?<seasonEpisode>s?{season(?:\:0+)?}(?<episodeSeparator>e|x)?(?<episode>{episode(?:\:0+)?}))(?<separator>.+?(?={))?",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FileNameBuilder(INamingConfigService namingConfigService, IConfigService configService, Logger logger)
        {
            _namingConfigService = namingConfigService;
            _configService = configService;
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
                Parser.Parser.CleanupEpisodeTitle(sortedEpisodes.First().Title)
            };

            var tokenValues = new Dictionary<string, string>(FilenameBuilderTokenEqualityComparer.Instance)
            {
                {"{Series Title}", series.Title}
            };

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

            var seasonEpisode = SeasonEpisodePatternRegex.Match(pattern);
            if (seasonEpisode.Success)
            {
                var episodeFormat = new EpisodeFormat
                {
                    EpisodeSeparator = seasonEpisode.Groups["episodeSeparator"].Value,
                    Separator = seasonEpisode.Groups["separator"].Value,
                    EpisodePattern = seasonEpisode.Groups["episode"].Value,
                    SeasonEpisodePattern = seasonEpisode.Groups["seasonEpisode"].Value,
                };

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

                    episodeTitles.Add(Parser.Parser.CleanupEpisodeTitle(episode.Title));
                }

                seasonEpisodePattern = ReplaceNumberTokens(seasonEpisodePattern, sortedEpisodes);
                tokenValues.Add("{Season Episode}", seasonEpisodePattern);
            }  
            
            tokenValues.Add("{Episode Title}", String.Join(" + ", episodeTitles.Distinct()));
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
                    var tokenValues = new Dictionary<string, string>(FilenameBuilderTokenEqualityComparer.Instance);
                    tokenValues.Add("{Series Title}", series.Title);

                    seasonFolder = ReplaceSeasonTokens(_configService.SeasonFolderFormat, seasonNumber);
                    seasonFolder = ReplaceTokens(seasonFolder, tokenValues);
                }

                path = Path.Combine(path, seasonFolder);
            }

            return Path.Combine(path, fileName + extension);
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

            if (patternTokenArray.All(t => !char.IsLetter(t) || char.IsLower(t)))
            {
                replacementText = replacementText.ToLowerInvariant();
            }

            else if (patternTokenArray.All(t => !char.IsLetter(t) || char.IsUpper(t)))
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
    }

    public enum MultiEpisodeStyle
    {
        Extend = 0,
        Duplicate = 1,
        Repeat = 2,
        Scene = 3
    }
}