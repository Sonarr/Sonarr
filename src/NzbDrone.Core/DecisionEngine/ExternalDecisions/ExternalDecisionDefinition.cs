using System;
using Equ;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public class ExternalDecisionDefinition : ProviderDefinition, IEquatable<ExternalDecisionDefinition>
    {
        public const int DefaultPriority = 25;

        private static readonly MemberwiseEqualityComparer<ExternalDecisionDefinition> Comparer = MemberwiseEqualityComparer<ExternalDecisionDefinition>.ByProperties;

        public ExternalDecisionType DecisionType { get; set; }
        public int Priority { get; set; } = DefaultPriority;

        public bool Equals(ExternalDecisionDefinition other)
        {
            return Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ExternalDecisionDefinition);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }
    }
}
