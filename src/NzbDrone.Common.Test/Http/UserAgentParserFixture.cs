using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http;

[TestFixture]
public class UserAgentParserFixture : TestBase
{
    // Ref *Arr `_userAgent = $"{BuildInfo.AppName}/{BuildInfo.Version} ({osName} {osVersion})";`
    // Ref Mylar `Mylar3/' +str(hash) +'(' +vers +') +http://www.github.com/mylar3/mylar3/`
    [TestCase("Mylar3/ 3ee23rh23irqfq (13123123) http://www.github.com/mylar3/mylar3/", "Mylar3")]
    [TestCase("Lidarr/1.0.0.2300 (ubuntu 20.04)", "Lidarr")]
    [TestCase("Radarr/1.0.0.2300 (ubuntu 20.04)", "Radarr")]
    [TestCase("Readarr/1.0.0.2300 (ubuntu 20.04)", "Readarr")]
    [TestCase("Sonarr/3.0.6.9999 (ubuntu 20.04)", "Sonarr")]
    [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36", "Other")]
    public void should_parse_user_agent(string userAgent, string parsedAgent)
    {
        UserAgentParser.ParseSource(userAgent).Should().Be(parsedAgent);
    }
}
