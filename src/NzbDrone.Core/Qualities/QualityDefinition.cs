using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.Qualities
{
    public class QualityDefinition : ModelBase
    {
        public Quality Quality { get; set; }

        public string Title { get; set; }

        public int Weight { get; set; }

        public int MinSize { get; set; }
        public int MaxSize { get; set; }

        public QualityDefinition()
        {

        }

        public QualityDefinition(Quality quality)
        {
            Quality = quality;
            Title = quality.Name;
        }

        public override string ToString()
        {
            return Quality.Name;
        }
    }
}