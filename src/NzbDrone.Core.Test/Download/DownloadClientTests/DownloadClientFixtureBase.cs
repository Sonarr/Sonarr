﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.IndexerTests;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.DownloadClientTests
{
    public abstract class DownloadClientFixtureBase<TSubject> : CoreTest<TSubject>
        where TSubject : class, IDownloadClient
    {
        protected readonly string _title = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE";
        protected readonly string _downloadUrl = "http://somewhere.com/Droned.S01E01.Pilot.1080p.WEB-DL-DRONE.ext";

        [SetUp]
        public void SetupBase()
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.DownloadClientHistoryLimit)
                .Returns(30);

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), (SearchCriteriaBase)null))
                .Returns(() => CreateRemoteEpisode());

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.GetAsync(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => Task.FromResult(new HttpResponse(r, new HttpHeader(), Array.Empty<byte>())));

            Mocker.GetMock<IRemotePathMappingService>()
                .Setup(v => v.RemapRemoteToLocal(It.IsAny<string>(), It.IsAny<OsPath>()))
                .Returns<string, OsPath>((h, r) => r);
        }

        protected virtual RemoteEpisode CreateRemoteEpisode()
        {
            var remoteEpisode = new RemoteEpisode();
            remoteEpisode.Release = new ReleaseInfo();
            remoteEpisode.Release.Title = _title;
            remoteEpisode.Release.DownloadUrl = _downloadUrl;
            remoteEpisode.Release.DownloadProtocol = Subject.Protocol;

            remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            remoteEpisode.ParsedEpisodeInfo.FullSeason = false;

            remoteEpisode.Episodes = new List<Episode>();

            remoteEpisode.Series = new Series();

            return remoteEpisode;
        }

        protected virtual IIndexer CreateIndexer()
        {
            return new TestIndexer(Mocker.Resolve<IHttpClient>(),
                Mocker.Resolve<IIndexerStatusService>(),
                Mocker.Resolve<IConfigService>(),
                Mocker.Resolve<IParsingService>(),
                Mocker.Resolve<Logger>(),
                Mocker.Resolve<ILocalizationService>());
        }

        protected void VerifyIdentifiable(DownloadClientItem downloadClientItem)
        {
            downloadClientItem.DownloadClientInfo.Protocol.Should().Be(Subject.Protocol);
            downloadClientItem.DownloadClientInfo.Id.Should().Be(Subject.Definition.Id);
            downloadClientItem.DownloadClientInfo.Name.Should().Be(Subject.Definition.Name);
            downloadClientItem.DownloadId.Should().NotBeNullOrEmpty();
            downloadClientItem.Title.Should().NotBeNullOrEmpty();
        }

        protected void VerifyQueued(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);
            downloadClientItem.RemainingSize.Should().NotBe(0);

            // downloadClientItem.RemainingTime.Should().NotBe(TimeSpan.Zero);
            // downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Queued);
        }

        protected void VerifyPaused(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.RemainingSize.Should().NotBe(0);

            // downloadClientItem.RemainingTime.Should().NotBe(TimeSpan.Zero);
            // downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Paused);
        }

        protected void VerifyDownloading(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.RemainingSize.Should().NotBe(0);

            // downloadClientItem.RemainingTime.Should().NotBe(TimeSpan.Zero);
            // downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Downloading);
        }

        protected void VerifyPostprocessing(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            // downloadClientItem.RemainingTime.Should().NotBe(TimeSpan.Zero);
            // downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Downloading);
        }

        protected void VerifyCompleted(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.Title.Should().NotBeNullOrEmpty();
            downloadClientItem.RemainingSize.Should().Be(0);
            downloadClientItem.RemainingTime.Should().Be(TimeSpan.Zero);

            // downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Completed);
        }

        protected void VerifyWarning(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.Status.Should().Be(DownloadItemStatus.Warning);
        }

        protected void VerifyFailed(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.Status.Should().Be(DownloadItemStatus.Failed);
        }
    }
}
