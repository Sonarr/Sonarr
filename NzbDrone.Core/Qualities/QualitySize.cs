using System.Linq;
using NzbDrone.Core.Datastore;
using ServiceStack.DataAnnotations;


namespace NzbDrone.Core.Qualities
{
    [Alias("QualitySizes")]
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