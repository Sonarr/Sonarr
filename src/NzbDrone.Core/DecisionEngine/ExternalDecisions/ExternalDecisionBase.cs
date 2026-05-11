using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public abstract class ExternalDecisionBase<TSettings> : IExternalDecision
        where TSettings : ExternalDecisionSettingsBase<TSettings>, new()
    {
        public abstract string Name { get; }
        public Type ConfigContract => typeof(TSettings);
        public virtual ProviderMessage Message => null;
        public IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();
        public ProviderDefinition Definition { get; set; }

        protected TSettings Settings => (TSettings)Definition.Settings;
        protected ExternalDecisionDefinition DecisionDefinition => (ExternalDecisionDefinition)Definition;

        public virtual ExternalRejectionResponse EvaluateRejection(ExternalRejectionRequest request)
        {
            return new ExternalRejectionResponse();
        }

        public virtual ExternalPrioritizationResponse EvaluatePrioritization(ExternalPrioritizationRequest request)
        {
            return new ExternalPrioritizationResponse();
        }

        public abstract ValidationResult Test();

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
