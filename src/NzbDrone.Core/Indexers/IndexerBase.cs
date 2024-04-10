using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerBase<TSettings> : IIndexer
        where TSettings : IIndexerSettings, new()
    {
        private static readonly Regex MultiRegex = new (@"[_. ](?<multi>multi)[_. ]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected readonly IIndexerStatusService _indexerStatusService;
        protected readonly IConfigService _configService;
        protected readonly IParsingService _parsingService;
        protected readonly Logger _logger;
        protected readonly ILocalizationService _localizationService;

        public abstract string Name { get; }
        public abstract DownloadProtocol Protocol { get; }
        public int Priority { get; set; }
        public int SeasonSearchMaximumSingleEpisodeAge { get; set; }

        public abstract bool SupportsRss { get; }
        public abstract bool SupportsSearch { get; }

        public IndexerBase(IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
        {
            _indexerStatusService = indexerStatusService;
            _configService = configService;
            _parsingService = parsingService;
            _logger = logger;
            _localizationService = localizationService;
        }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public virtual IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                var config = (IProviderConfig)new TSettings();

                yield return new IndexerDefinition
                {
                    Name = GetType().Name,
                    EnableRss = config.Validate().IsValid && SupportsRss,
                    EnableAutomaticSearch = config.Validate().IsValid && SupportsSearch,
                    EnableInteractiveSearch = config.Validate().IsValid && SupportsSearch,
                    Implementation = GetType().Name,
                    Settings = config
                };
            }
        }

        public virtual ProviderDefinition Definition { get; set; }

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public abstract Task<IList<ReleaseInfo>> FetchRecent();
        public abstract Task<IList<ReleaseInfo>> Fetch(SeasonSearchCriteria searchCriteria);
        public abstract Task<IList<ReleaseInfo>> Fetch(SingleEpisodeSearchCriteria searchCriteria);
        public abstract Task<IList<ReleaseInfo>> Fetch(DailyEpisodeSearchCriteria searchCriteria);
        public abstract Task<IList<ReleaseInfo>> Fetch(DailySeasonSearchCriteria searchCriteria);
        public abstract Task<IList<ReleaseInfo>> Fetch(AnimeEpisodeSearchCriteria searchCriteria);
        public abstract Task<IList<ReleaseInfo>> Fetch(AnimeSeasonSearchCriteria searchCriteria);
        public abstract Task<IList<ReleaseInfo>> Fetch(SpecialEpisodeSearchCriteria searchCriteria);
        public abstract HttpRequest GetDownloadRequest(string link);

        protected virtual IList<ReleaseInfo> CleanupReleases(IEnumerable<ReleaseInfo> releases)
        {
            var result = releases.DistinctBy(v => v.Guid).ToList();
            var settings = Definition.Settings as IIndexerSettings;

            result.ForEach(c =>
            {
                // Use multi languages from setting if ReleaseInfo languages is empty
                if (c.Languages.Empty() && MultiRegex.IsMatch(c.Title) && settings.MultiLanguages.Any())
                {
                    c.Languages = settings.MultiLanguages.Select(i => (Language)i).ToList();
                }

                c.IndexerId = Definition.Id;
                c.Indexer = Definition.Name;
                c.DownloadProtocol = Protocol;
                c.IndexerPriority = ((IndexerDefinition)Definition).Priority;
                c.SeasonSearchMaximumSingleEpisodeAge = ((IndexerDefinition)Definition).SeasonSearchMaximumSingleEpisodeAge;
            });

            return result;
        }

        public ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                Test(failures).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Test aborted due to exception");
                failures.Add(new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationTestAbortedDueToError", new Dictionary<string, object> { { "exceptionMessage", ex.Message } })));
            }

            return new ValidationResult(failures);
        }

        protected abstract Task Test(List<ValidationFailure> failures);

        public override string ToString()
        {
            return Definition.Name;
        }
    }
}
