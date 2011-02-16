using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Indexer
    {
        [SubSonicPrimaryKey(true)]
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
