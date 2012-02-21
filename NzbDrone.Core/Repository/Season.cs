using System;
using NzbDrone.Core.Model;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("Seasons")]
    [PrimaryKey("SeasonId", autoIncrement = true)]
    public class Season
    {
        public int SeasonId { get; set; }
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public Boolean Ignored { get; set; }
    }
}