using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck
{
    [TestFixture]
    public class HealthCheckFixture : CoreTest
    {
        private const string WikiRoot = "https://wiki.servarr.com/";
        [TestCase("I blew up because of some weird user mistake", null, WikiRoot + "Sonarr_System#i_blew_up_because_of_some_weird_user_mistake")]
        [TestCase("I blew up because of some weird user mistake", "#my_health_check", WikiRoot + "Sonarr_System#my_health_check")]
        [TestCase("I blew up because of some weird user mistake", "Custom_Page#my_health_check", WikiRoot + "Custom_Page#my_health_check")]
        public void should_format_wiki_url(string message, string wikiFragment, string expectedUrl)
        {
            var subject = new NzbDrone.Core.HealthCheck.HealthCheck(typeof(HealthCheckBase), HealthCheckResult.Warning, message, wikiFragment);

            subject.WikiUrl.Should().Be(expectedUrl);
        }
    }
}
