using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Localization
{
    public interface ILocalizationService
    {
        Dictionary<string, string> GetLocalizationDictionary();

        string GetLocalizedString(string phrase);
        string GetLocalizedString(string phrase, Dictionary<string, object> tokens);
        string GetLanguageIdentifier();
    }

    public class LocalizationService : ILocalizationService, IHandleAsync<ConfigSavedEvent>
    {
        private const string DefaultCulture = "en";
        private static readonly Regex TokenRegex = new Regex(@"(?:\{)(?<token>[a-z0-9]+)(?:\})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly ICached<Dictionary<string, string>> _cache;

        private readonly IConfigService _configService;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly Logger _logger;

        public LocalizationService(IConfigService configService,
                                   IAppFolderInfo appFolderInfo,
                                   ICacheManager cacheManager,
                                   Logger logger)
        {
            _configService = configService;
            _appFolderInfo = appFolderInfo;
            _cache = cacheManager.GetCache<Dictionary<string, string>>(typeof(Dictionary<string, string>), "localization");
            _logger = logger;
        }

        public Dictionary<string, string> GetLocalizationDictionary()
        {
            var language = GetLanguageFileName();

            return GetLocalizationDictionary(language);
        }

        public string GetLocalizedString(string phrase)
        {
            return GetLocalizedString(phrase, new Dictionary<string, object>());
        }

        public string GetLocalizedString(string phrase, Dictionary<string, object> tokens)
        {
            var language = GetLanguageFileName();

            if (string.IsNullOrEmpty(phrase))
            {
                throw new ArgumentNullException(nameof(phrase));
            }

            if (language == null)
            {
                language = DefaultCulture;
            }

            var dictionary = GetLocalizationDictionary(language);

            if (dictionary.TryGetValue(phrase, out var value))
            {
                return ReplaceTokens(value, tokens);
            }

            return phrase;
        }

        public string GetLanguageIdentifier()
        {
            var isoLanguage = IsoLanguages.Get((Language)_configService.UILanguage) ?? IsoLanguages.Get(Language.English);
            var language = isoLanguage.TwoLetterCode;

            if (isoLanguage.CountryCode.IsNotNullOrWhiteSpace())
            {
                language = $"{language}-{isoLanguage.CountryCode.ToUpperInvariant()}";
            }

            return language;
        }

        private string ReplaceTokens(string input, Dictionary<string, object> tokens)
        {
            tokens.TryAdd("appName", "Sonarr");

            return TokenRegex.Replace(input, (match) =>
            {
                var tokenName = match.Groups["token"].Value;

                tokens.TryGetValue(tokenName, out var token);

                return token?.ToString() ?? $"{{{tokenName}}}";
            });
        }

        private string GetLanguageFileName()
        {
            return GetLanguageIdentifier().Replace("-", "_").ToLowerInvariant();
        }

        private Dictionary<string, string> GetLocalizationDictionary(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentNullException(nameof(language));
            }

            var startupFolder = _appFolderInfo.StartUpFolder;

            var prefix = Path.Combine(startupFolder, "Localization", "Core");
            var key = prefix + language;

            return _cache.Get("localization", () => GetDictionary(prefix, language, DefaultCulture + ".json").GetAwaiter().GetResult());
        }

        private async Task<Dictionary<string, string>> GetDictionary(string prefix, string culture, string baseFilename)
        {
            if (string.IsNullOrEmpty(culture))
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var baseFilenamePath = Path.Combine(prefix, baseFilename);

            var alternativeFilenamePath = Path.Combine(prefix, GetResourceFilename(culture));

            await CopyInto(dictionary, baseFilenamePath).ConfigureAwait(false);

            if (culture.Contains('_'))
            {
                var languageBaseFilenamePath = Path.Combine(prefix, GetResourceFilename(culture.Split('_')[0]));
                await CopyInto(dictionary, languageBaseFilenamePath).ConfigureAwait(false);
            }

            await CopyInto(dictionary, alternativeFilenamePath).ConfigureAwait(false);

            return dictionary;
        }

        private async Task CopyInto(IDictionary<string, string> dictionary, string resourcePath)
        {
            if (!File.Exists(resourcePath))
            {
                _logger.Error("Missing translation/culture resource: {0}", resourcePath);
                return;
            }

            await using var fs = File.OpenRead(resourcePath);
            var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);

            foreach (var key in dict.Keys)
            {
                dictionary[key] = dict[key];
            }
        }

        private static string GetResourceFilename(string culture)
        {
            var parts = culture.Split('_');

            if (parts.Length == 2)
            {
                culture = parts[0].ToLowerInvariant() + "_" + parts[1].ToUpperInvariant();
            }
            else
            {
                culture = culture.ToLowerInvariant();
            }

            return culture + ".json";
        }

        public void HandleAsync(ConfigSavedEvent message)
        {
            _cache.Clear();
        }
    }
}
