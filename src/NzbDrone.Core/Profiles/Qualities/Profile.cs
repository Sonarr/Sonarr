using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class Profile : ModelBase
    {
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileQualityItem> Items { get; set; }

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

        public QualityIndex GetIndex(Quality quality)
        {
            return GetIndex(quality.Id);
        }

        public QualityIndex GetIndex(int id)
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
                        return new QualityIndex(i, g);
                    }
                }
            }

            return new QualityIndex();
        }
    }
}
