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

            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.UpdateUrl).Returns("http://update.nzbdrone.com/_release/");

            var updates = Subject.GetAvailablePackages().ToList();

            updates.Should().NotBeEmpty();
            updates.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.FileName));
            updates.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Url));
            updates.Should().OnlyContain(c => c.Version != null);
            updates.Should().OnlyContain(c => c.Version.Minor != 0);
        }
    }
}
