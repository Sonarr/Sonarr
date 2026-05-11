using System;
using Equ;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public abstract class ExternalDecisionSettingsBase<TSettings> : IProviderConfig, IEquatable<TSettings>
        where TSettings : ExternalDecisionSettingsBase<TSettings>
    {
        private static readonly MemberwiseEqualityComparer<TSettings> Comparer = MemberwiseEqualityComparer<TSettings>.ByProperties;

        public abstract NzbDroneValidationResult Validate();

        public bool Equals(TSettings other)
        {
            return Comparer.Equals(this as TSettings, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TSettings);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this as TSettings);
        }
    }
}
