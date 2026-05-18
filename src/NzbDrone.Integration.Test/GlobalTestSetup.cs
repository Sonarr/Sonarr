using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test
{
    [SetUpFixture]
    public class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            NzbDroneRunner.EnsureUiContent();
        }
    }
}
