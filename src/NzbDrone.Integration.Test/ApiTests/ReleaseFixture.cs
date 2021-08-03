using System.Linq;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Sonarr.Api.V3.Indexers;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    [Ignore("Need mock Newznab to test")]
    public class ReleaseFixture : IntegrationTest
    {
        [Test]
        public void should_only_have_unknown_series_releases()
        {
            var releases = Releases.All();
            var indexers = Indexers.All();

            releases.Should().OnlyContain(c => c.Rejections.Contains("Unknown Series"));
            releases.Should().OnlyContain(c => BeValidRelease(c));
        }

        [Test]
        public void should_reject_unknown_release()
        {
            var result = Releases.Post(new ReleaseResource { Guid = "unknown" }, HttpStatusCode.NotFound);

            result.Id.Should().Be(0);
        }

        [Test]
        public void should_accept_request_with_only_guid_supplied()
        {
            var releases = Releases.All();

            // InternalServerError is caused by the Release being invalid for download (no Series).
            // But if it didn't accept it, it would return NotFound.
            // TODO: Maybe we should create a full mock Newznab server endpoint.
            //var result = Releases.Post(new ReleaseResource { Guid = releases.First().Guid });
            //result.Guid.Should().Be(releases.First().Guid);

            var result = Releases.Post(new ReleaseResource { Guid = releases.First().Guid }, HttpStatusCode.InternalServerError);
        }

        private bool BeValidRelease(ReleaseResource releaseResource)
        {
            releaseResource.Guid.Should().NotBeNullOrEmpty();
            releaseResource.Age.Should().BeGreaterOrEqualTo(-1);
            releaseResource.Title.Should().NotBeNullOrWhiteSpace();
            releaseResource.DownloadUrl.Should().NotBeNullOrWhiteSpace();
            releaseResource.SeriesTitle.Should().NotBeNullOrWhiteSpace();

            //TODO: uncomment these after moving to restsharp for rss
            //releaseResource.NzbInfoUrl.Should().NotBeNullOrWhiteSpace();
            //releaseResource.Size.Should().BeGreaterThan(0);

            return true;
        }
    }
}
