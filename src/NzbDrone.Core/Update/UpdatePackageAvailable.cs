using System;

namespace NzbDrone.Core.Update
{
    public class UpdatePackageAvailable
    {
        public Boolean Available { get; set; }
        public UpdatePackage UpdatePackage { get; set; }
    }
}
