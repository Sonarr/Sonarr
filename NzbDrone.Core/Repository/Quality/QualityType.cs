using System.Linq;
using PetaPoco;

namespace NzbDrone.Core.Repository.Quality
{
    [TableName("QualityTypes")]
    [PrimaryKey("QualityTypeId", autoIncrement = false)]
    public class QualityType
    {
        public int QualityTypeId { get; set; }
        public string Name { get; set; }
        public int MinSize { get; set; }
        public int MaxSize { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}