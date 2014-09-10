using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Test.UpdateTests
{
    public class UpdatePackageProviderFixture : CoreTest<UpdatePackageProvider>
    {
        [Test]
        public void no_update_when_version_higher()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("master", new Version(10, 0)).Should().BeNull();
        }

        [Test]
        public void finds_update_when_version_lower()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("master", new Version(2, 0)).Should().NotBeNull();
        }


        [Test]
        public void should_get_recent_updates()
        {
            const string branch = "master";
            UseRealHttp();
            var recent = Subject.GetRecentUpdates(branch, 2);

            recent.Should().NotBeEmpty();
            recent.Should().OnlyContain(c => c.Hash.IsNotNullOrWhiteSpace());
            recent.Should().OnlyContain(c => c.FileName.StartsWith("http://update.nzbdrone.com/"));
            recent.Should().OnlyContain(c => c.ReleaseDate.Year == 2014);
            recent.Should().OnlyContain(c => c.Changes.New != null);
            recent.Should().OnlyContain(c => c.Changes.Fixed != null);
        }
    }
}
