using Equ;
using Workarr.Validation;

namespace Workarr.ImportLists
{
    public abstract class ImportListSettingsBase<TSettings> : IImportListSettings, IEquatable<TSettings>
        where TSettings : ImportListSettingsBase<TSettings>
    {
        private static readonly MemberwiseEqualityComparer<TSettings> Comparer = MemberwiseEqualityComparer<TSettings>.ByProperties;

        public abstract string BaseUrl { get; set; }

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
