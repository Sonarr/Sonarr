using System;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class IndexerSetting
    {
        [SubSonicPrimaryKey(true)]
        public int Id { get; set; }

        public Boolean Enable { get; set; }

        public String IndexProviderType { get; set; }

        public String Name { get; set; }
    }
}