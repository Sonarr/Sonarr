using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class ExternalNotificationSetting
    {
        [SubSonicPrimaryKey(true)]
        public int Id { get; set; }

        public bool Enabled { get; set; }
        public string NotifierName { get; set; }
        public string Name { get; set; }
    }
}
