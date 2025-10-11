using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Test.Common;
using Sonarr.Api.V3.Indexers;

namespace NzbDrone.Api.Test.v3.Indexers;

[TestFixture]
public class ReleasePushControllerFixture : TestBase<ReleasePushController>
{
    [SetUp]
    public void SetupController()
    {
        var qualityProfile = new QualityProfile
        {
            Items = new List<QualityProfileQualityItem>
            {
                new()
                {
                    Allowed = true,
                    Quality = Quality.Bluray720p
                }
            }
        };

        Mocker.SetConstant(LogManager.GetLogger(nameof(ReleasePushControllerFixture)));

        Mocker.GetMock<IQualityProfileService>()
              .Setup(x => x.GetDefaultProfile(It.IsAny<string>(), It.IsAny<Quality>(), It.IsAny<Quality[]>()))
              .Returns(qualityProfile);
    }

    private ReleaseResource BuildRelease(Action<ReleaseResource> configure = null)
    {
        var resource = new ReleaseResource
        {
            Title = "Test Release",
            DownloadUrl = "https://example.com/release.torrent",
            PublishDate = DateTime.UtcNow,
            Protocol = DownloadProtocol.Torrent
        };

        configure?.Invoke(resource);

        return resource;
    }

    [Test]
    public void should_fail_when_download_client_name_unknown()
    {
        Mocker.GetMock<IDownloadClientFactory>()
              .Setup(x => x.All())
              .Returns(new List<DownloadClientDefinition>());

        var release = BuildRelease(r => r.DownloadClient = "missing-client");

        var exception = Assert.Throws<ValidationException>(() => Subject.Create(release));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Errors.Select(e => e.PropertyName), Does.Contain("DownloadClient"));
    }

    [Test]
    public void should_fail_when_download_client_name_disabled()
    {
        var disabledClient = new DownloadClientDefinition
        {
            Id = 5,
            Name = "Disabled Client",
            Enable = false
        };

        Mocker.GetMock<IDownloadClientFactory>()
              .Setup(x => x.All())
              .Returns(new List<DownloadClientDefinition> { disabledClient });

        var release = BuildRelease(r => r.DownloadClient = "Disabled Client");

        var exception = Assert.Throws<ValidationException>(() => Subject.Create(release));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Errors.Select(e => e.PropertyName), Does.Contain("DownloadClient"));
    }

    [Test]
    public void should_fail_when_download_client_id_unknown()
    {
        const int requestedId = 42;

        Mocker.GetMock<IDownloadClientFactory>()
              .Setup(x => x.Get(requestedId))
              .Throws(new ModelNotFoundException(typeof(DownloadClientDefinition), requestedId));

        var release = BuildRelease(r => r.DownloadClientId = requestedId);

        var exception = Assert.Throws<ValidationException>(() => Subject.Create(release));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Errors.Select(e => e.PropertyName), Does.Contain("DownloadClientId"));
    }

    [Test]
    public void should_fail_when_download_client_id_disabled()
    {
        const int requestedId = 11;

        var disabledClient = new DownloadClientDefinition
        {
            Id = requestedId,
            Name = "Disabled Client",
            Enable = false
        };

        Mocker.GetMock<IDownloadClientFactory>()
              .Setup(x => x.Get(requestedId))
              .Returns(disabledClient);

        var release = BuildRelease(r => r.DownloadClientId = requestedId);

        var exception = Assert.Throws<ValidationException>(() => Subject.Create(release));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Errors.Select(e => e.PropertyName), Does.Contain("DownloadClientId"));
    }

    [Test]
    public void should_fail_when_download_client_name_and_id_mismatch()
    {
        const int requestedId = 7;
        var definitionByName = new DownloadClientDefinition
        {
            Id = 21,
            Name = "Known Client",
            Enable = true
        };
        var definitionById = new DownloadClientDefinition
        {
            Id = requestedId,
            Name = "Different Client",
            Enable = true
        };

        Mocker.GetMock<IDownloadClientFactory>()
              .Setup(x => x.All())
              .Returns(new List<DownloadClientDefinition> { definitionByName });

        Mocker.GetMock<IDownloadClientFactory>()
              .Setup(x => x.Get(requestedId))
              .Returns(definitionById);

        var release = BuildRelease(r =>
        {
            r.DownloadClient = "Known Client";
            r.DownloadClientId = requestedId;
        });

        var exception = Assert.Throws<ValidationException>(() => Subject.Create(release));

        Assert.That(exception, Is.Not.Null);
        var properties = exception!.Errors.Select(e => e.PropertyName).ToList();
        Assert.That(properties, Does.Contain("DownloadClient"));
        Assert.That(properties, Does.Contain("DownloadClientId"));
    }
}
