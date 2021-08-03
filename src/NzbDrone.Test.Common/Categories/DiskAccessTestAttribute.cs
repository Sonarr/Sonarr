using NUnit.Framework;

namespace NzbDrone.Test.Common.Categories
{
    public class DiskAccessTestAttribute : CategoryAttribute
    {
        public DiskAccessTestAttribute()
            : base("DiskAccessTest")
        {
        }
    }
}
