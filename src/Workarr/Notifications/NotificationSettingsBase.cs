using Equ;
using Workarr.ThingiProvider;
using Workarr.Validation;

namespace Workarr.Notifications
{
    public abstract class NotificationSettingsBase<TSettings> : IProviderConfig, IEquatable<TSettings>
        where TSettings : NotificationSettingsBase<TSettings>
    {
        private static readonly MemberwiseEqualityComparer<TSettings> Comparer = MemberwiseEqualityComparer<TSettings>.ByProperties;

        public abstract WorkarrValidationResult Validate();

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
