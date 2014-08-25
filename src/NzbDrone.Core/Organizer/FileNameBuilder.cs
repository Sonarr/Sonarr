using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFileName(List<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig namingConfig = null);
        string BuildFilePath(Series series, Int32 seasonNumber, String fileName, String extension);
        BasicNamingConfig GetBasicNamingConfig(NamingConfig nameSpec);
        string GetSeriesFolder(Series series, NamingConfig namingConfig = null);
        string GetSeasonFolder(Series series, Int32 seasonNumber, NamingConfig namingConfig = null);
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly ICached<EpisodeFormat> _patternCache;
        private readonly Logger _logger;

        private static readonly Regex TitleRegex = new Regex(@"\{(?<prefix>[- ._]*)(?<token>(?:[a-z0-9]+)(?:(?<separator>[- ._]+)(?:[a-z0-9]+))?)(?::(?<customFormat>[a-z0-9]+))?(?<suffix>[- ._]*)\}",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EpisodeRegex = new Regex(@"(?<episode>\{episode(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SeasonRegex = new Regex(@"(?<season>\{season(?:\:0+)?})",
                                                              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AbsoluteEpisodeRegex = new Regex(@"(?<absolute>\{absolute(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SeasonEpisodePatternRegex = new Regex(@"(?<separator>(?<=}).+?)?(?<seasonEpisode>s?{season(?:\:0+)?}(?<episodeSeparator>e|x)(?<episode>{episode(?:\:0+)?}))(?<separator>.+?(?={))?",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AbsoluteEpisodePatternRegex = new Regex(@"(?<separator>(?<=}).+?)?(?<absolute>{absolute(?:\:0+)?})(?<separator>.+?(?={))?",
                                                                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AirDateRegex = new Regex(@"\{Air(\s|\W|_)Date\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SeriesTitleRegex = new Regex(@"(?<token>\{(?:Series)(?<separator>\s|\.|-|_)(Clean)?Title\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FileNameCleanupRegex = new Regex(@"\.{2,}", RegexOptions.Compiled);

        private static readonly char[] EpisodeTitleTrimCharacters = new[] { ' ', '.', '?' };

        public FileNameBuilder(INamingConfigService namingConfigService,
                               IQualityDefinitionService qualityDefinitionService,
                               ICacheManager cacheManager,
                               Logger logger)
        {
            _namingConfigService = namingConfigService;
            _qualityDefinitionService = qualityDefinitionService;
            _patternCache = cacheManager.GetCache<EpisodeFormat>(GetType());
            _logger = logger;
        }

        public string BuildFileName(List<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            if (!namingConfig.RenameEpisodes)
            {
                if (episodeFile.SceneName.IsNullOrWhiteSpace())
                {
                    return Path.GetFileNameWithoutExtension(episodeFile.RelativePath);
                }

                return episodeFile.SceneName;
            }

            if (namingConfig.StandardEpisodeFormat.IsNullOrWhiteSpace() && series.SeriesType == SeriesTypes.Standard)
            {
                throw new NamingFormatException("Standard episode format cannot be null");
            }

            if (namingConfig.DailyEpisodeFormat.IsNullOrWhiteSpace() && series.SeriesType == SeriesTypes.Daily)
            {
                throw new NamingFormatException("Daily episode format cannot be null");
            }

            var pattern = namingConfig.StandardEpisodeFormat;

            var tokenHandlers = new Dictionary<String, Func<TokenMatch, String>>(FileNameBuilderTokenEqualityComparer.Instance);

            episodes = episodes.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber).ToList();

            AddSeriesTokens(tokenHandlers, series);

            AddEpisodeTokens(tokenHandlers, episodes);

            AddEpisodeFileTokens(tokenHandlers, episodeFile);

            AddMediaInfoTokens(tokenHandlers, episodeFile);
            
            if (series.SeriesType == SeriesTypes.Daily)
            {
                pattern = namingConfig.DailyEpisodeFormat;
            }

            if (series.SeriesType == SeriesTypes.Anime && episodes.All(e => e.AbsoluteEpisodeNumber > 0))
            {
                pattern = namingConfig.AnimeEpisodeFormat;
            }

            var episodeFormat = GetEpisodeFormat(pattern);

            if (episodeFormat != null)
            {
                pattern = pattern.Replace(episodeFormat.SeasonEpisodePattern, "{Season Episode}");
                var seasonEpisodePattern = episodeFormat.SeasonEpisodePattern;

                foreach (var episode in episodes.Skip(1))
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
                }

                seasonEpisodePattern = ReplaceNumberTokens(seasonEpisodePattern, episodes);
                tokenHandlers["{Season Episode}"] = m => seasonEpisodePattern;
            }

            //TODO: Extract to another method
            var absoluteEpisodeFormat = GetAbsoluteFormat(pattern);

            if (absoluteEpisodeFormat != null)
            {
                if (series.SeriesType != SeriesTypes.Anime)
                {
                    pattern = pattern.Replace(absoluteEpisodeFormat.AbsoluteEpisodePattern, "");
                }

                else
                {
                    pattern = pattern.Replace(absoluteEpisodeFormat.AbsoluteEpisodePattern, "{Absolute Pattern}");
                    var absoluteEpisodePattern = absoluteEpisodeFormat.AbsoluteEpisodePattern;

                    foreach (var episode in episodes.Skip(1))
                    {
                        switch ((MultiEpisodeStyle)namingConfig.MultiEpisodeStyle)
                        {
                            case MultiEpisodeStyle.Duplicate:
                                absoluteEpisodePattern += absoluteEpisodeFormat.Separator +
                                                          absoluteEpisodeFormat.AbsoluteEpisodePattern;
                                break;

                            case MultiEpisodeStyle.Repeat:
                                absoluteEpisodePattern += absoluteEpisodeFormat.Separator +
                                                          absoluteEpisodeFormat.AbsoluteEpisodePattern;
                                break;

                            case MultiEpisodeStyle.Scene:
                                absoluteEpisodePattern += "-" + absoluteEpisodeFormat.AbsoluteEpisodePattern;
                                break;

                            //MultiEpisodeStyle.Extend
                            default:
                                absoluteEpisodePattern += "-" + absoluteEpisodeFormat.AbsoluteEpisodePattern;
                                break;
                        }
                    }

                    absoluteEpisodePattern = ReplaceAbsoluteNumberTokens(absoluteEpisodePattern, episodes);
                    tokenHandlers["{Absolute Pattern}"] = m => absoluteEpisodePattern;
                }
            }

            var filename = ReplaceTokens(pattern, tokenHandlers).Trim();
            filename = FileNameCleanupRegex.Replace(filename, match => match.Captures[0].Value[0].ToString());

            return filename;
        }

        public string BuildFilePath(Series series, int seasonNumber, string fileName, string extension)
        {
            Ensure.That(extension, () => extension).IsNotNullOrWhiteSpace();

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
                    seasonFolder = GetSeasonFolder(series, seasonNumber, nameSpec);
                }

                seasonFolder = CleanFileName(seasonFolder);

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

        public string GetSeriesFolder(Series series, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var tokenHandlers = new Dictionary<string, Func<TokenMatch, String>>(FileNameBuilderTokenEqualityComparer.Instance);

            AddSeriesTokens(tokenHandlers, series);

            return ReplaceTokens(namingConfig.SeriesFolderFormat, tokenHandlers);
        }

        public string GetSeasonFolder(Series series, Int32 seasonNumber, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var tokenHandlers = new Dictionary<string, Func<TokenMatch, String>>(FileNameBuilderTokenEqualityComparer.Instance);

            AddSeriesTokens(tokenHandlers, series);

            AddSeasonTokens(tokenHandlers, seasonNumber);

            return ReplaceTokens(namingConfig.SeasonFolderFormat, tokenHandlers);
        }

        public static string CleanTitle(string name)
        {
            string[] dropCharacters = { ":", ".", "(", ")" };

            string result = name;

            for (int i = 0; i < dropCharacters.Length; i++)
            {
                result = result.Replace(dropCharacters[i], "");
            }

            return result;
        }

        public static string CleanFileName(string name)
        {
            string result = name;
            string[] badCharacters = { "\\", "/", "<", ">", "?", "*", ":", "|", "\"" };
            string[] goodCharacters = { "+", "+", "", "", "!", "-", "-", "", "" };

            for (int i = 0; i < badCharacters.Length; i++)
            {
                result = result.Replace(badCharacters[i], goodCharacters[i]);
            }

            return result.Trim();
        }

        private void AddSeriesTokens(Dictionary<String, Func<TokenMatch, String>> tokenHandlers, Series series)
        {
            tokenHandlers["{Series Title}"] = m => series.Title;
            tokenHandlers["{Series CleanTitle}"] = m => CleanTitle(series.Title);
        }

        private void AddSeasonTokens(Dictionary<String, Func<TokenMatch, String>> tokenHandlers, Int32 seasonNumber)
        {
            tokenHandlers["{Season}"] = m => seasonNumber.ToString(m.CustomFormat ?? "0");
        }

        private void AddEpisodeTokens(Dictionary<String, Func<TokenMatch, String>> tokenHandlers, List<Episode> episodes)
        {
            if (!episodes.First().AirDate.IsNullOrWhiteSpace())
            {
                tokenHandlers["{Air Date}"] = m => episodes.First().AirDate.Replace('-', ' ');
            }
            else
            {
                tokenHandlers["{Air Date}"] = m => "Unknown";
            }

            tokenHandlers["{Episode Title}"] = m => GetEpisodeTitle(episodes);
        }

        private void AddEpisodeFileTokens(Dictionary<String, Func<TokenMatch, String>> tokenHandlers, EpisodeFile episodeFile)
        {
            tokenHandlers["{Original Title}"]= m => episodeFile.SceneName;
            tokenHandlers["{Release Group}"] = m => episodeFile.ReleaseGroup;
            tokenHandlers["{Quality Title}"] = m => GetQualityTitle(episodeFile.Quality);
        }

        private void AddMediaInfoTokens(Dictionary<String, Func<TokenMatch, String>> tokenHandlers, EpisodeFile episodeFile)
        {
            if (episodeFile.MediaInfo == null) return;

            String mediaInfoVideo;
            switch (episodeFile.MediaInfo.VideoCodec)
            {
                case "AVC":
                    // TODO: What to do if the original SceneName is hashed?
                    if (!episodeFile.SceneName.IsNullOrWhiteSpace() && Path.GetFileNameWithoutExtension(episodeFile.SceneName).Contains("h264"))
                    {
                        mediaInfoVideo = "h264";
                    }
                    else
                    {
                        mediaInfoVideo = "x264";
                    }
                    break;

                default:
                    mediaInfoVideo = episodeFile.MediaInfo.VideoCodec;
                    break;
            }

            String mediaInfoAudio;
            switch (episodeFile.MediaInfo.AudioFormat)
            {
                case "AC-3":
                    mediaInfoAudio = "AC3";
                    break;

                case "MPEG Audio":
                    if (episodeFile.MediaInfo.AudioProfile == "Layer 3")
                    {
                        mediaInfoAudio = "MP3";
                    }
                    else
                    {
                        mediaInfoAudio = episodeFile.MediaInfo.AudioFormat;
                    }
                    break;

                case "DTS":
                    mediaInfoAudio = episodeFile.MediaInfo.AudioFormat;
                    break;

                default:
                    mediaInfoAudio = episodeFile.MediaInfo.AudioFormat;
                    break;
            }

            var mediaInfoAudioLanguages = GetLanguagesToken(episodeFile.MediaInfo.AudioLanguages);
            if (!mediaInfoAudioLanguages.IsNullOrWhiteSpace())
            {
                mediaInfoAudioLanguages = String.Format("[{0}]", mediaInfoAudioLanguages);
            }

            if (mediaInfoAudioLanguages == "[EN]")
            {
                mediaInfoAudioLanguages = String.Empty;
            }

            var mediaInfoSubtitleLanguages = GetLanguagesToken(episodeFile.MediaInfo.Subtitles);
            if (!mediaInfoSubtitleLanguages.IsNullOrWhiteSpace())
            {
                mediaInfoSubtitleLanguages = String.Format("[{0}]", mediaInfoSubtitleLanguages);
            }

            tokenHandlers["{MediaInfo Video}"] = m => mediaInfoVideo;
            tokenHandlers["{MediaInfo Audio}"] = m => mediaInfoAudio;

            tokenHandlers["{MediaInfo Simple}"] = m => String.Format("{0} {1}", mediaInfoVideo, mediaInfoAudio);

            tokenHandlers["{MediaInfo Full}"] = m => String.Format("{0} {1}{2} {3}", mediaInfoVideo, mediaInfoAudio, mediaInfoAudioLanguages, mediaInfoSubtitleLanguages);
        }

        private string GetLanguagesToken(String mediaInfoLanguages)
        {
            List<string> tokens = new List<string>();
            foreach (var item in mediaInfoLanguages.Split('/'))
            {
                if (!string.IsNullOrWhiteSpace(item))
                    tokens.Add(item.Trim());
            }

            var cultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures);
            for (int i = 0; i < tokens.Count; i++)
            {
                try
                {
                    var cultureInfo = cultures.FirstOrDefault(p => p.EnglishName == tokens[i]);

                    if (cultureInfo != null)
                        tokens[i] = cultureInfo.TwoLetterISOLanguageName.ToUpper();
                }
                catch
                {
                }
            }

            return string.Join("+", tokens.Distinct());
        }

        private string ReplaceTokens(String pattern, Dictionary<String, Func<TokenMatch, String>> tokenHandlers)
        {
            return TitleRegex.Replace(pattern, match => ReplaceToken(match, tokenHandlers));
        }

        private string ReplaceToken(Match match, Dictionary<String, Func<TokenMatch, String>> tokenHandlers)
        {
            var tokenMatch = new TokenMatch
            {
                RegexMatch = match,
                Prefix = match.Groups["prefix"].Value,
                Separator = match.Groups["separator"].Value,
                Suffix = match.Groups["suffix"].Value,
                Token = match.Groups["token"].Value,
                CustomFormat = match.Groups["customFormat"].Value
            };

            if (tokenMatch.CustomFormat.IsNullOrWhiteSpace())
            {
                tokenMatch.CustomFormat = null;
            }

            var tokenHandler = tokenHandlers.GetValueOrDefault(tokenMatch.Token, m => String.Empty);

            var replacementText = tokenHandler(tokenMatch).Trim();

            if (tokenMatch.Token.All(t => !Char.IsLetter(t) || Char.IsLower(t)))
            {
                replacementText = replacementText.ToLower();
            }
            else if (tokenMatch.Token.All(t => !Char.IsLetter(t) || Char.IsUpper(t)))
            {
                replacementText = replacementText.ToUpper();
            }

            if (!tokenMatch.Separator.IsNullOrWhiteSpace())
            {
                replacementText = replacementText.Replace(" ", tokenMatch.Separator);
            }

            replacementText = CleanFileName(replacementText);

            if (!replacementText.IsNullOrWhiteSpace())
            {
                replacementText = tokenMatch.Prefix + replacementText + tokenMatch.Suffix;
            }

            return replacementText;
        }


        private sealed class TokenMatch
        {
            public Match RegexMatch { get; set; }
            public String Prefix { get; set; }
            public String Separator { get; set; }
            public String Suffix { get; set; }
            public String Token { get; set; }
            public String CustomFormat { get; set; }
        }

        private string ReplaceNumberTokens(string pattern, List<Episode> episodes)
        {
            var episodeIndex = 0;
            pattern = EpisodeRegex.Replace(pattern, match =>
            {
                var episode = episodes[episodeIndex];
                episodeIndex++;

                return ReplaceNumberToken(match.Groups["episode"].Value, episode.EpisodeNumber);
            });

            return ReplaceSeasonTokens(pattern, episodes.First().SeasonNumber);
        }

        private string ReplaceAbsoluteNumberTokens(string pattern, List<Episode> episodes)
        {
            var episodeIndex = 0;
            pattern = AbsoluteEpisodeRegex.Replace(pattern, match =>
            {
                var episode = episodes[episodeIndex];
                episodeIndex++;

                //TODO: We need to handle this null check somewhere, I think earlier is better...
                return ReplaceNumberToken(match.Groups["absolute"].Value, episode.AbsoluteEpisodeNumber.Value);
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

        private AbsoluteEpisodeFormat GetAbsoluteFormat(string pattern)
        {
            var match = AbsoluteEpisodePatternRegex.Match(pattern);

            if (match.Success)
            {
                return new AbsoluteEpisodeFormat
                       {
                           Separator = match.Groups["separator"].Value,
                           AbsoluteEpisodePattern = match.Groups["absolute"].Value
                       };
            }

            return null;
        }

        private String GetEpisodeTitle(List<Episode> episodes)
        {
            if (episodes.Count == 1)
            {
                return episodes.First().Title.TrimEnd(EpisodeTitleTrimCharacters);
            }

            var titles = episodes
                .Select(c => c.Title.TrimEnd(EpisodeTitleTrimCharacters))
                .Select(Parser.Parser.CleanupEpisodeTitle)
                .Distinct();

            return String.Join(" + ", titles);
        }

        private String GetQualityTitle(QualityModel quality)
        {
            if (quality.Proper)
                return _qualityDefinitionService.Get(quality.Quality).Title + " Proper";
            else
                return _qualityDefinitionService.Get(quality.Quality).Title;
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