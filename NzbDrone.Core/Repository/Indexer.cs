using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Indexer
    {
        [SubSonicPrimaryKey]
        public virtual int IndexerId { get; set; }

        public string IndexerName { get; set; }
        public string RssUrl { get; set; }

        [SubSonicNullStringAttribute]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ApiUrl { get; set; }

        public bool Enabled { get; set; }
        public int Order { get; set; }

        [SubSonicToManyRelation]
        public virtual List<History> Histories { get; private set; }
    }
}