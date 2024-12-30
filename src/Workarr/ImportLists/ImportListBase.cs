using FluentValidation.Results;
using NLog;
using Workarr.Configuration;
using Workarr.Localization;
using Workarr.Parser;
using Workarr.Parser.Model;
using Workarr.ThingiProvider;

namespace Workarr.ImportLists
{
    public class ImportListFetchResult
    {
        public ImportListFetchResult()
        {
            Series = new List<ImportListItemInfo>();
        }

        public ImportListFetchResult(IEnumerable<ImportListItemInfo> series, bool anyFailure)
        {
            Series = series.ToList();
            AnyFailure = anyFailure;
        }

        public List<ImportListItemInfo> Series { get; set; }
        public bool AnyFailure { get; set; }
    }

    public abstract class ImportListBase<TSettings> : IImportList
        where TSettings : IImportListSettings, new()
    {
        protected readonly IImportListStatusService _importListStatusService;
        protected readonly IConfigService _configService;
        protected readonly IParsingService _parsingService;
        protected readonly ILocalizationService _localizationService;
        protected readonly Logger _logger;

        public abstract string Name { get; }

        public abstract ImportListType ListType { get; }

        public abstract TimeSpan MinRefreshInterval { get; }

        public ImportListBase(IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, ILocalizationService localizationService, Logger logger)
        {
            _importListStatusService = importListStatusService;
            _configService = configService;
            _parsingService = parsingService;
            _localizationService = localizationService;
            _logger = logger;
        }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public virtual IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                var config = (IProviderConfig)new TSettings();

                yield return new ImportListDefinition
                {
                    EnableAutomaticAdd = config.Validate().IsValid,
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

        public abstract ImportListFetchResult Fetch();

        protected virtual IList<ImportListItemInfo> CleanupListItems(IEnumerable<ImportListItemInfo> releases)
        {
            var result = releases.DistinctBy(r => new { r.Title, r.TvdbId, r.ImdbId }).ToList();

            result.ForEach(c =>
            {
                c.ImportListId = Definition.Id;
                c.ImportList = Definition.Name;
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
                failures.Add(new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("ImportListsValidationTestFailed", new Dictionary<string, object> { { "exceptionMessage", ex.Message } })));
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
