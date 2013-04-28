using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class ReleaseIntegrationTest : IntegrationTest
    {
        [Test]
        public void should_only_have_unknown_series_releases()
        {
            Releases.All().Should().OnlyContain(c => c.Rejections.Contains("Unknown Series"));
        }


    }
}