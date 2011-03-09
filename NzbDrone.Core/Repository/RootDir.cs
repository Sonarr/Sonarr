using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class RootDir
    {
        [SubSonicPrimaryKey(true)]
        public int RootDirId { get; set; }

        public string Path { get; set; }

        public bool Default { get; set; }
    }
}
