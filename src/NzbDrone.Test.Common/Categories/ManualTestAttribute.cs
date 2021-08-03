using NUnit.Framework;

namespace NzbDrone.Test.Common.Categories
{
    public class ManualTestAttribute : CategoryAttribute
    {
        public ManualTestAttribute()
            : base("ManualTest")
        {
        }
    }
}
