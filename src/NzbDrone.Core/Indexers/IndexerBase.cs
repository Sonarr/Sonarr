using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerBase<TSettings> : IIndexer
        where TSettings : IIndexerSettings, new()
    {
        protected readonly IIndexerStatusService _indexerStatusService;
        protected readonly IConfigService _configService;
        protected readonly IParsingService _parsingService;
        protected readonly Logger _logger;

        public abstract string Name { get; }
        public abstract DownloadProtocol Protocol { get; }

        public abstract bool SupportsRss { get; }
        public abstract bool SupportsSearch { get; }

        public IndexerBase(IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _configService = configService;
            _parsingService = parsingService;
            _logger = logger;
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

        public virtual object RequestAction(string action, IDictionary<string, string> query) { return null; }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public abstract IList<ReleaseInfo> FetchRecent();
        public abstract IList<ReleaseInfo> Fetch(SeasonSearchCriteria searchCriteria);
        public abstract IList<ReleaseInfo> Fetch(SingleEpisodeSearchCriteria searchCriteria);
        public abstract IList<ReleaseInfo> Fetch(DailyEpisodeSearchCriteria searchCriteria);
        public abstract IList<ReleaseInfo> Fetch(DailySeasonSearchCriteria searchCriteria);
        public abstract IList<ReleaseInfo> Fetch(AnimeEpisodeSearchCriteria searchCriteria);
        public abstract IList<ReleaseInfo> Fetch(SpecialEpisodeSearchCriteria searchCriteria);

        protected virtual IList<ReleaseInfo> CleanupReleases(IEnumerable<ReleaseInfo> releases)
        {
            var result = releases.DistinctBy(v => v.Guid).ToList();

            result.ForEach(c =>
            {
                c.IndexerId = Definition.Id;
                c.Indexer = Definition.Name;
                c.DownloadProtocol = Protocol;
            });

            return result;
        }

        public ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                Test(failures);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Test aborted due to exception");
                failures.Add(new ValidationFailure(string.Empty, "Test was aborted due to an error: " + ex.Message));
            }

            return new ValidationResult(failures);
        }

        protected abstract void Test(List<ValidationFailure> failures);

        public override string ToString()
        {
            return Definition.Name;
        }
    }
}
