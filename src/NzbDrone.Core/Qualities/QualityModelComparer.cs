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
            Ensure.That(qualityProfile.Allowed, () => qualityProfile.Allowed).HasItems();

            _qualityProfile = qualityProfile;
        }

        public int Compare(Quality left, Quality right)
        {
            int leftIndex = _qualityProfile.Allowed.IndexOf(left);
            int rightIndex = _qualityProfile.Allowed.IndexOf(right);

            return leftIndex.CompareTo(rightIndex);
        }

        public int Compare(QualityModel left, QualityModel right)
        {
            int result = Compare(left.Quality, right.Quality);

            if (result == 0)
                result = left.Proper.CompareTo(right.Proper);

            return result;
        }
        /*
        public string GetName(Quality quality)
        {
            QualityDefinition qualityDefinition = _qualityDefinitionService.Get(quality);

            return qualityDefinition.Name;
        }

        public string GetName(QualityModel quality)
        {
            QualityDefinition qualityDefinition = _qualityDefinitionService.Get(quality.Quality);

            if (quality.Proper)
                return qualityDefinition.Name + " Proper";
            else
                return qualityDefinition.Name;
        }

        public string GetSceneName(QualityModel quality)
        {
            QualityDefinition qualityDefinition = _qualityDefinitionService.Get(quality.Quality);

            if (quality.Proper)
                return qualityDefinition.SceneName + " PROPER";
            else
                return qualityDefinition.SceneName;
        }*/
    }
}
