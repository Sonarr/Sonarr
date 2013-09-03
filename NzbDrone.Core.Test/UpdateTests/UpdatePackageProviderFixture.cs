using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;
using System.Linq;

namespace NzbDrone.Core.Test.UpdateTests
{
    public class UpdatePackageProviderFixture : CoreTest<UpdatePackageProvider>
    {
        [Test]
        public void should_get_list_of_available_updates()
        {
            UseRealHttp();

            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.Branch).Returns("master");

            Subject.GetLatestUpdate().Should().BeNull();
        }
    }
}
