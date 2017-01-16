namespace NzbDrone.Core.Update
{
    public class UpdatePackageAvailable
    {
        public bool Available { get; set; }
        public UpdatePackage UpdatePackage { get; set; }
    }
}
