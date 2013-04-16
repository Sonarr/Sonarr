using NUnit.Framework;

namespace NzbDrone.Test.Common
{
    public class IntegrationTestAttribute : CategoryAttribute
    {
        public IntegrationTestAttribute()
            : base("Integration Test")
        {

        }
    }
}