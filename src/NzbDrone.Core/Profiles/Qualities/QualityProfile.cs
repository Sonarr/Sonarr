using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class QualityProfile : ModelBase
    {
        public QualityProfile()
        {
            FormatItems = new List<ProfileFormatItem>();
        }

        public string Name { get; set; }
        public bool UpgradeAllowed { get; set; }
        public int Cutoff { get; set; }
        public int MinFormatScore { get; set; }
        public int CutoffFormatScore { get; set; }
        public List<ProfileFormatItem> FormatItems { get; set; }
        public List<QualityProfileQualityItem> Items { get; set; }

        public Quality FirststAllowedQuality()
        {
            var firstAllowed = Items.First(q => q.Allowed);

            if (firstAllowed.Quality != null)
            {
                return firstAllowed.Quality;
            }

            // Returning any item from the group will work,
            // returning the first because it's the true first quality.
            return firstAllowed.Items.First().Quality;
        }

        public Quality LastAllowedQuality()
        {
            var lastAllowed = Items.Last(q => q.Allowed);

            if (lastAllowed.Quality != null)
            {
                return lastAllowed.Quality;
            }

            // Returning any item from the group will work,
            // returning the last because it's the true last quality.
            return lastAllowed.Items.Last().Quality;
        }

        public QualityIndex GetIndex(Quality quality, bool respectGroupOrder = false)
        {
            return GetIndex(quality.Id, respectGroupOrder);
        }

        public QualityIndex GetIndex(int id, bool respectGroupOrder = false)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var quality = item.Quality;

                // Quality matches by ID
                if (quality != null && quality.Id == id)
                {
                    return new QualityIndex(i);
                }

                // Group matches by ID
                if (item.Id > 0 && item.Id == id)
                {
                    return new QualityIndex(i);
                }

                for (var g = 0; g < item.Items.Count; g++)
                {
                    var groupItem = item.Items[g];

                    if (groupItem.Quality.Id == id)
                    {
                        return respectGroupOrder ? new QualityIndex(i, g) : new QualityIndex(i);
                    }
                }
            }

            return new QualityIndex();
        }

        public int CalculateCustomFormatScore(List<CustomFormat> formats)
        {
            return FormatItems.Where(x => formats.Contains(x.Format)).Sum(x => x.Score);
        }
    }
}
