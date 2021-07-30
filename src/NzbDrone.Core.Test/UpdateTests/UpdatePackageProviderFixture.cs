using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Test.UpdateTests
{
    public class UpdatePackageProviderFixture : CoreTest<UpdatePackageProvider>
    {
        [SetUp]
        public void Setup()
        {
            if (OsInfo.Os == Os.LinuxMusl || OsInfo.Os == Os.Bsd)
            {
                throw new IgnoreException("Ignore until we have musl releases");
            }

            Mocker.GetMock<IPlatformInfo>().SetupGet(c => c.Version).Returns(new Version("9.9.9"));
        }

        [Test]
        public void no_update_when_version_higher()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("main", new Version(10, 0)).Should().BeNull();
        }

        [Test]
        public void finds_update_when_version_lower()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("main", new Version(3, 0)).Should().NotBeNull();
        }

        [Test]
        public void should_get_master_if_branch_doesnt_exit()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("invalid_branch", new Version(3, 0)).Should().NotBeNull();
        }


        [Test]
        public void should_get_recent_updates()
        {
            const string branch = "main";
            UseRealHttp();
            var recent = Subject.GetRecentUpdates(branch, new Version(3, 0), null);

            recent.Should().NotBeEmpty();
            recent.Should().OnlyContain(c => c.Hash.IsNotNullOrWhiteSpace());
            recent.Should().OnlyContain(c => c.FileName.Contains($"Sonarr.{c.Branch}.3."));
            recent.Should().OnlyContain(c => c.ReleaseDate.Year >= 2014);
            recent.Where(c => c.Changes != null).Should().OnlyContain(c => c.Changes.New != null);
            recent.Where(c => c.Changes != null).Should().OnlyContain(c => c.Changes.Fixed != null);
        }
    }
}
