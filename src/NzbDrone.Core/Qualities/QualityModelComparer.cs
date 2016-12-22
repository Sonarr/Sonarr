using System.Collections.Generic;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Qualities
{
    public class QualityModelComparer : IComparer<Quality>, IComparer<QualityModel>
    {
        private readonly Profile _profile;

        public QualityModelComparer(Profile profile)
        {
            Ensure.That(profile, () => profile).IsNotNull();
            Ensure.That(profile.Items, () => profile.Items).HasItems();

            _profile = profile;
        }

        public int Compare(Quality left, Quality right)
        {
            int leftIndex = _profile.Items.FindIndex(v => v.Quality == left);
            int rightIndex = _profile.Items.FindIndex(v => v.Quality == right);

            return leftIndex.CompareTo(rightIndex);
        }

        public int Compare(QualityModel left, QualityModel right)
        {
            int result = Compare(left.Quality, right.Quality);

            if (result == 0)
            {
                result = left.Revision.CompareTo(right.Revision);
            }

            return result;
        }
    }
}
