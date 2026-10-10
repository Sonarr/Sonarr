using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class MultiSeasonImportFixture : CoreTest<CompletedDownloadService>
    {
        private const string Title = "Friends.S01-S03.1080p.BluRay.x264-GRP";

        private Series _series;
        private List<Episode> _episodes;
        private string _root;
        private TrackedDownload _trackedDownload;

        [SetUp]
        public void Setup()
        {
            _series = new Series
            {
                Id = 1,
                Title = "Friends",
                CleanTitle = "friends",
                SeriesType = SeriesTypes.Standard,
                SeasonType = SeasonType.Official,
                QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() },
                Path = @"C:\TV\Friends".AsOsAgnostic()
            };

            _episodes = Enumerable.Range(1, 3)
                .SelectMany(s => Enumerable.Range(1, 2).Select(e => new Episode { Id = (s * 10) + e, SeriesId = 1, SeasonNumber = s, EpisodeNumber = e }))
                .ToList();

            _root = Path.Combine(@"C:\Downloads".AsOsAgnostic(), Title);

            var files = _episodes.Select(e => Path.Combine(_root, $"Season {e.SeasonNumber}", $"Friends.S{e.SeasonNumber:00}E{e.EpisodeNumber:00}.1080p.BluRay.x264-GRP.mkv")).ToArray();

            _trackedDownload = new TrackedDownload
            {
                State = TrackedDownloadState.ImportPending,
                DownloadItem = new DownloadClientItem
                {
                    Title = Title,
                    DownloadId = "abc",
                    Status = DownloadItemStatus.Completed,
                    OutputPath = new OsPath(_root),
                    DownloadClientInfo = new DownloadClientItemClientInfo()
                },
                RemoteEpisode = new RemoteEpisode
                {
                    Series = _series,
                    Episodes = _episodes,
                    ParsedEpisodeInfo = Parser.Parser.ParseTitle(Title)
                }
            };

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisode(1, It.IsAny<int>(), It.IsAny<int>()))
                  .Returns<int, int, int>((_, s, e) => _episodes.SingleOrDefault(x => x.SeasonNumber == s && x.EpisodeNumber == e));

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesBySeason(1, It.IsAny<int>()))
                  .Returns<int, int>((_, s) => _episodes.Where(x => x.SeasonNumber == s).ToList());

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTitle(It.IsAny<string>()))
                  .Returns(_series);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.FilterExistingFiles(It.IsAny<List<string>>(), It.IsAny<Series>()))
                  .Returns<List<string>, Series>((f, _) => f);

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Add(It.IsAny<EpisodeFile>()))
                  .Returns<EpisodeFile>(f => f);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Setup(s => s.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), It.IsAny<LocalEpisode>(), It.IsAny<bool>()))
                  .Returns(new EpisodeFileMoveResult());

            Mocker.GetMock<IDetectSample>()
                  .Setup(s => s.IsSample(It.IsAny<Series>(), It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(DetectSampleResult.NotSample);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_root))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFileSize(It.IsAny<string>()))
                  .Returns(1000000000);

            Mocker.GetMock<IDiskScanService>()
                  .Setup(s => s.GetVideoFiles(_root, true))
                  .Returns(files);

            Mocker.GetMock<IDiskScanService>()
                  .Setup(s => s.FilterPaths(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()))
                  .Returns<string, IEnumerable<string>, bool>((_, f, _) => f.ToList());

            Mocker.GetMock<IProvideImportItemService>()
                  .Setup(s => s.ProvideImportItem(It.IsAny<DownloadClientItem>(), It.IsAny<DownloadClientItem>()))
                  .Returns<DownloadClientItem, DownloadClientItem>((i, _) => i);

            Mocker.SetConstant<IParsingService>(Mocker.Resolve<ParsingService>());

            Mocker.SetConstant<IEnumerable<IAggregateLocalEpisode>>(new IAggregateLocalEpisode[]
            {
                Mocker.Resolve<AggregateReleaseInfo>(),
                Mocker.Resolve<AggregateQuality>(),
                Mocker.Resolve<AggregateEpisodes>()
            });

            Mocker.SetConstant<IAggregationService>(Mocker.Resolve<AggregationService>());

            Mocker.SetConstant<IEnumerable<IImportDecisionEngineSpecification>>(new IImportDecisionEngineSpecification[]
            {
                Mocker.Resolve<FullSeasonSpecification>(),
                Mocker.Resolve<MatchesFolderSpecification>(),
                Mocker.Resolve<MatchesGrabSpecification>(),
                Mocker.Resolve<AlreadyImportedSpecification>()
            });

            Mocker.SetConstant<IMakeImportDecision>(Mocker.Resolve<ImportDecisionMaker>());
            Mocker.SetConstant<IImportApprovedEpisodes>(Mocker.Resolve<ImportApprovedEpisodes>());
            Mocker.SetConstant<IDownloadedEpisodesImportService>(Mocker.Resolve<DownloadedEpisodesImportService>());
        }

        [Test]
        public void should_import_every_season_of_a_completed_multi_season_download()
        {
            Subject.Import(_trackedDownload);

            foreach (var message in (_trackedDownload.StatusMessages ?? []).SelectMany(m => m.Messages.Select(x => $"{m.Title}: {x}")))
            {
                TestContext.Out.WriteLine(message);
            }

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(s => s.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), It.IsAny<LocalEpisode>(), It.IsAny<bool>()), Times.Exactly(6));

            _trackedDownload.State.Should().Be(TrackedDownloadState.Imported);
        }
    }
}
