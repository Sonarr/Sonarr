using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public interface IExternalDecisionFactory : IProviderFactory<IExternalDecision, ExternalDecisionDefinition>
    {
        List<IExternalDecision> RejectionDecisionsEnabled();
        List<IExternalDecision> PrioritizationDecisionsEnabled();
    }

    public class ExternalDecisionFactory : ProviderFactory<IExternalDecision, ExternalDecisionDefinition>, IExternalDecisionFactory
    {
        private readonly IExternalDecisionStatusService _externalDecisionStatusService;
        private readonly Logger _logger;

        public ExternalDecisionFactory(IExternalDecisionStatusService externalDecisionStatusService,
                                   IExternalDecisionRepository providerRepository,
                                   IEnumerable<IExternalDecision> providers,
                                   IServiceProvider container,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _externalDecisionStatusService = externalDecisionStatusService;
            _logger = logger;
        }

        protected override List<ExternalDecisionDefinition> Active()
        {
            return base.Active().Where(c => c.Enable).ToList();
        }

        public List<IExternalDecision> RejectionDecisionsEnabled()
        {
            return FilterBlockedDecisions(GetAvailableProviders()
                .Where(h => ((ExternalDecisionDefinition)h.Definition).DecisionType == ExternalDecisionType.Rejection))
                .OrderBy(h => ((ExternalDecisionDefinition)h.Definition).Priority)
                .ThenBy(h => h.Definition.Id)
                .ToList();
        }

        public List<IExternalDecision> PrioritizationDecisionsEnabled()
        {
            return FilterBlockedDecisions(GetAvailableProviders()
                .Where(h => ((ExternalDecisionDefinition)h.Definition).DecisionType == ExternalDecisionType.Prioritization))
                .OrderBy(h => ((ExternalDecisionDefinition)h.Definition).Priority)
                .ThenBy(h => h.Definition.Id)
                .ToList();
        }

        private IEnumerable<IExternalDecision> FilterBlockedDecisions(IEnumerable<IExternalDecision> decisions)
        {
            var blockedDecisions = _externalDecisionStatusService.GetBlockedProviders().ToDictionary(v => v.ProviderId, v => v);

            foreach (var decision in decisions)
            {
                if (blockedDecisions.TryGetValue(decision.Definition.Id, out var status))
                {
                    _logger.Debug("Temporarily ignoring external decision {0} till {1} due to recent failures.", decision.Definition.Name, status.DisabledTill.Value.ToLocalTime());
                    continue;
                }

                yield return decision;
            }
        }

        public override ValidationResult Test(ExternalDecisionDefinition definition)
        {
            var result = base.Test(definition);

            if (definition.Id == 0)
            {
                return result;
            }

            if (result == null || result.IsValid)
            {
                _externalDecisionStatusService.RecordSuccess(definition.Id);
            }
            else
            {
                _externalDecisionStatusService.RecordFailure(definition.Id);
            }

            return result;
        }
    }
}
