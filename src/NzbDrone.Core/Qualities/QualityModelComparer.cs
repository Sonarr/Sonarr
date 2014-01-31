using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Core.Qualities
{
    public class QualityModelComparer : IComparer<Quality>, IComparer<QualityModel>
    {
        private readonly QualityProfile _qualityProfile;

        public QualityModelComparer(QualityProfile qualityProfile)
        {
            Ensure.That(qualityProfile, () => qualityProfile).IsNotNull();
            Ensure.That(qualityProfile.Items, () => qualityProfile.Items).HasItems();

            _qualityProfile = qualityProfile;
        }

        public int Compare(Quality left, Quality right)
        {
            int leftIndex = _qualityProfile.Items.FindIndex(v => v.Quality == left);
            int rightIndex = _qualityProfile.Items.FindIndex(v => v.Quality == right);

            return leftIndex.CompareTo(rightIndex);
        }

        public int Compare(QualityModel left, QualityModel right)
        {
            int result = Compare(left.Quality, right.Quality);

            if (result == 0)
                result = left.Proper.CompareTo(right.Proper);

            return result;
        }
    }
}
