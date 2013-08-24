using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Update
{
    public class UpdatePackageAvailable
    {
        public Boolean Available { get; set; }
        public UpdatePackage UpdatePackage { get; set; }
    }
}
