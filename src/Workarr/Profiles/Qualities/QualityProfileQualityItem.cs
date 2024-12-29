using System.Text.Json.Serialization;
using Workarr.Datastore;
using Workarr.Extensions;
using Workarr.Qualities;

namespace Workarr.Profiles.Qualities
{
    public class QualityProfileQualityItem : IEmbeddedDocument
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
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
