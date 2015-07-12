using System.Collections.Generic;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Languages
{
    public class LanguageComparer : IComparer<Language>
    {
        private readonly LanguageProfile _profile;

        public LanguageComparer(LanguageProfile profile)
        {
            Ensure.That(profile, () => profile).IsNotNull();
            Ensure.That(profile.Languages, () => profile.Languages).HasItems();

            _profile = profile;
        }

        public int Compare(Language left, Language right)
        {
            int leftIndex = _profile.Languages.FindIndex(v => v.Language == left);
            int rightIndex = _profile.Languages.FindIndex(v => v.Language == right);

            return leftIndex.CompareTo(rightIndex);
        }
    }
}
