using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class QualityProfileQualityItem : IEmbeddedDocument
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }
        public Quality Quality { get; set; }
        public List<QualityProfileQualityItem> Items { get; set; }
        public bool Allowed { get; set; }

        public QualityProfileQualityItem()
        {
            Items = new List<QualityProfileQualityItem>();
        }

        public List<Quality> GetQualities()
        {
            if (Quality == null)
            {
                return Items.Select(s => s.Quality).ToList();
            }

            return new List<Quality> { Quality };
        }

        public override string ToString()
        {
            var qualitiesString = string.Join(", ", GetQualities());

            if (Name.IsNotNullOrWhiteSpace())
            {
                return $"{Name} ({qualitiesString})";
            }

            return qualitiesString;
        }
    }
}
