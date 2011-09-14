using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        public long MinSize { get; set; }
        public long MaxSize { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}