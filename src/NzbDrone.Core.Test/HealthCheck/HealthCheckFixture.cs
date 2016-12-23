using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck
{
    [TestFixture]
    public class HealthCheckFixture : CoreTest
    {
        private const string WikiRoot = "https://github.com/Sonarr/Sonarr/wiki/";

        [TestCase("I blew up because of some weird user mistake", null, WikiRoot + "Health-checks#i-blew-up-because-of-some-weird-user-mistake")]
        [TestCase("I blew up because of some weird user mistake", "#my-health-check", WikiRoot + "Health-checks#my-health-check")]
        [TestCase("I blew up because of some weird user mistake", "Custom-Page#my-health-check", WikiRoot + "Custom-Page#my-health-check")]
        public void should_format_wiki_url(string message, string wikiFragment, string expectedUrl)
        {
            var subject = new NzbDrone.Core.HealthCheck.HealthCheck(typeof(HealthCheckBase), HealthCheckResult.Warning, message, wikiFragment);

            subject.WikiUrl.Should().Be(expectedUrl);
        }
    }
}
