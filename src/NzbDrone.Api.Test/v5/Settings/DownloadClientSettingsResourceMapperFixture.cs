using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using Sonarr.Api.V5.Settings;

namespace NzbDrone.Api.Test.v5.Settings;

[TestFixture]
public class DownloadClientSettingsResourceMapperFixture
{
    [Test]
    public void should_map_retry_usenet_releases_by_guid()
    {
        var configService = new Mock<IConfigService>();
        configService.SetupGet(s => s.RetryUsenetReleasesByGuid).Returns(true);

        var resource = DownloadClientSettingsResourceMapper.ToResource(configService.Object);

        Assert.That(resource.RetryUsenetReleasesByGuid, Is.True);
    }
}
