using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListFactory : IProviderFactory<IImportList, ImportListDefinition>
    {
        List<IImportList> AutomaticAddEnabled(bool filterBlockedImportLists = true);
    }

    public class ImportListFactory : ProviderFactory<IImportList, ImportListDefinition>, IImportListFactory
    {
        private readonly IImportListStatusService _importListStatusService;
        private readonly Logger _logger;

        public ImportListFactory(IImportListStatusService importListStatusService,
                              IImportListRepository providerRepository,
                              IEnumerable<IImportList> providers,
                              IServiceProvider container,
                              IEventAggregator eventAggregator,
                              Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _importListStatusService = importListStatusService;
            _logger = logger;
        }

        protected override List<ImportListDefinition> Active()
        {
            return base.Active().Where(c => c.Enable).ToList();
        }

        public override void SetProviderCharacteristics(IImportList provider, ImportListDefinition definition)
        {
            base.SetProviderCharacteristics(provider, definition);

            definition.ListType = provider.ListType;
            definition.MinRefreshInterval = provider.MinRefreshInterval;
        }

        public List<IImportList> AutomaticAddEnabled(bool filterBlockedImportLists = true)
        {
            var enabledImportLists = GetAvailableProviders().Where(n => ((ImportListDefinition)n.Definition).EnableAutomaticAdd);

            if (filterBlockedImportLists)
            {
                return FilterBlockedImportLists(enabledImportLists).ToList();
            }

            return enabledImportLists.ToList();
        }

        private IEnumerable<IImportList> FilterBlockedImportLists(IEnumerable<IImportList> importLists)
        {
            var blockedImportLists = _importListStatusService.GetBlockedProviders().ToDictionary(v => v.ProviderId, v => v);

            foreach (var importList in importLists)
            {
                if (blockedImportLists.TryGetValue(importList.Definition.Id, out var blockedImportListStatus))
                {
                    _logger.Debug("Temporarily ignoring import list {0} till {1} due to recent failures.", importList.Definition.Name, blockedImportListStatus.DisabledTill.Value.ToLocalTime());
                    continue;
                }

                yield return importList;
            }
        }

        public override ValidationResult Test(ImportListDefinition definition)
        {
            var result = base.Test(definition);

            if (definition.Id == 0)
            {
                return result;
            }

            if (result == null || result.IsValid)
            {
                _importListStatusService.RecordSuccess(definition.Id);
            }
            else
            {
                _importListStatusService.RecordFailure(definition.Id);
            }

            return result;
        }
    }
}
