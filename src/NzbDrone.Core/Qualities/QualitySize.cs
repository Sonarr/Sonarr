using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.Qualities
{
    public class QualitySize : ModelBase
    {
        public int QualityId { get; set; }
        public string Name { get; set; }
        public int MinSize { get; set; }
        public int MaxSize { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}