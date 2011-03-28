using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class RootDir
    {
        public int Id { get; set; }

        public string Path { get; set; }
    }
}
