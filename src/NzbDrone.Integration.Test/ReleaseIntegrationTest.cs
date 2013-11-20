using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Indexers;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class ReleaseIntegrationTest : IntegrationTest
    {
        [Test]
        public void should_only_have_unknown_series_releases()
        {

            var releases = Releases.All();
            var indexers = Indexers.All();


            releases.Should().OnlyContain(c => c.Rejections.Contains("Unknown Series"));
            releases.Should().OnlyContain(c => BeValidRelease(c));
        }



        private bool BeValidRelease(ReleaseResource releaseResource)
        {
            releaseResource.Age.Should().BeGreaterOrEqualTo(-1);
            releaseResource.Title.Should().NotBeBlank();
            releaseResource.DownloadUrl.Should().NotBeBlank();
            releaseResource.SeriesTitle.Should().NotBeBlank();
            //TODO: uncomment these after moving to restsharp for rss
            //releaseResource.NzbInfoUrl.Should().NotBeBlank();
            //releaseResource.Size.Should().BeGreaterThan(0);

            return true;
        }

    }
}