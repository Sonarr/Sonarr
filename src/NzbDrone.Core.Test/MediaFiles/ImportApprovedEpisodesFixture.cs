using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class ImportApprovedEpisodesFixture : CoreTest<ImportApprovedEpisodes>
    {
        private List<ImportDecision> _rejectedDecisions;
        private List<ImportDecision> _approvedDecisions;

        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _rejectedDecisions = new List<ImportDecision>();
            _approvedDecisions = new List<ImportDecision>();

            var series = Builder<Series>.CreateNew()
                                        .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                        .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                        .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                                           .Build();



            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), new Rejection("Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), new Rejection("Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), new Rejection("Rejected!")));

            foreach (var episode in episodes)
            {
                _approvedDecisions.Add(new ImportDecision
                                           (
                                           new LocalEpisode
                                               {
                                                   Series = series,
                                                   Episodes = new List<Episode> { episode },
                                                   Path = Path.Combine(series.Path, "30 Rock - S01E01 - Pilot.avi"),
                                                   Quality = new QualityModel(Quality.Bluray720p),
                                                   ReleaseGroup = "DRONE"
                                               }));
            }

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Setup(s => s.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), It.IsAny<LocalEpisode>(), It.IsAny<bool>()))
                  .Returns(new EpisodeFileMoveResult());

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(new List<EpisodeFile>());
        }

        [Test]
        public void should_not_import_any_if_there_are_no_approved_decisions()
        {
            Subject.Import(_rejectedDecisions, false).Where(i => i.Result == ImportResultType.Imported).Should().BeEmpty();

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.IsAny<EpisodeFile>()), Times.Never());
        }

        [Test]
        public void should_import_each_approved()
        {
            Subject.Import(_approvedDecisions, false).Should().HaveCount(5);
        }

        [Test]
        public void should_only_import_approved()
        {
            var all = new List<ImportDecision>();
            all.AddRange(_rejectedDecisions);
            all.AddRange(_approvedDecisions);

            var result = Subject.Import(all, false);

            result.Should().HaveCount(all.Count);
            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_only_import_each_episode_once()
        {
            var all = new List<ImportDecision>();
            all.AddRange(_approvedDecisions);
            all.Add(new ImportDecision(_approvedDecisions.First().LocalEpisode));

            var result = Subject.Import(all, false);

            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_move_new_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), _approvedDecisions.First().LocalEpisode, false),
                          Times.Once());
        }

        [Test]
        public void should_publish_EpisodeImportedEvent_for_new_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<EpisodeImportedEvent>()), Times.Once());
        }

        [Test]
        public void should_not_move_existing_files()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, false);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), _approvedDecisions.First().LocalEpisode, false),
                          Times.Never());
        }

        [Test]
        public void should_use_nzb_title_as_scene_name()
        {
            _downloadClientItem.Title = "malcolm.in.the.middle.s02e05.dvdrip.xvid-ingot";

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<EpisodeFile>(c => c.SceneName == _downloadClientItem.Title)));
        }

        [TestCase(".mkv")]
        [TestCase(".par2")]
        [TestCase(".nzb")]
        public void should_remove_extension_from_nzb_title_for_scene_name(string extension)
        {
            var title = "malcolm.in.the.middle.s02e05.dvdrip.xvid-ingot";

            _downloadClientItem.Title = title + extension;

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<EpisodeFile>(c => c.SceneName == title)));
        }

        [Test]
        public void should_not_use_nzb_title_as_scene_name_if_full_season()
        {
            _approvedDecisions.First().LocalEpisode.Path = "c:\\tv\\season1\\malcolm.in.the.middle.s02e23.dvdrip.xvid-ingot.mkv".AsOsAgnostic();
            _downloadClientItem.Title = "malcolm.in.the.middle.s02.dvdrip.xvid-ingot";

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, _downloadClientItem);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<EpisodeFile>(c => c.SceneName == "malcolm.in.the.middle.s02e23.dvdrip.xvid-ingot")));
        }

        [Test]
        public void should_use_file_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _approvedDecisions.First().LocalEpisode.Path = "c:\\tv\\malcolm.in.the.middle.s02e23.dvdrip.xvid-ingot.mkv".AsOsAgnostic();

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<EpisodeFile>(c => c.SceneName == "malcolm.in.the.middle.s02e23.dvdrip.xvid-ingot")));
        }

        [Test]
        public void should_not_use_file_name_as_scenename_if_it_doesnt_looks_like_scenename()
        {
            _approvedDecisions.First().LocalEpisode.Path = "c:\\tv\\aaaaa.mkv".AsOsAgnostic();

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.Is<EpisodeFile>(c => c.SceneName == null)));
        }

        [Test]
        public void should_import_larger_files_first()
        {
            var fileDecision = _approvedDecisions.First();
            fileDecision.LocalEpisode.Size = 1.Gigabytes();

            var sampleDecision = new ImportDecision
                (new LocalEpisode
                 {
                     Series = fileDecision.LocalEpisode.Series,
                     Episodes = new List<Episode> { fileDecision.LocalEpisode.Episodes.First() },
                     Path = @"C:\Test\TV\30 Rock\30 Rock - S01E01 - Pilot.avi".AsOsAgnostic(),
                     Quality = new QualityModel(Quality.Bluray720p),
                     Size = 80.Megabytes()
                 });


            var all = new List<ImportDecision>();
            all.Add(fileDecision);
            all.Add(sampleDecision);

            var results = Subject.Import(all, false);

            results.Should().HaveCount(all.Count);
            results.Should().ContainSingle(d => d.Result == ImportResultType.Imported);
            results.Should().ContainSingle(d => d.Result == ImportResultType.Imported && d.ImportDecision.LocalEpisode.Size == fileDecision.LocalEpisode.Size);
        }

        [Test]
        public void should_copy_when_cannot_move_files_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, new DownloadClientItem { Title = "30.Rock.S01E01", CanMoveFiles = false});

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), _approvedDecisions.First().LocalEpisode, true), Times.Once());
        }

        [Test]
        public void should_use_override_importmode()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, new DownloadClientItem { Title = "30.Rock.S01E01", CanMoveFiles = false }, ImportMode.Move);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), _approvedDecisions.First().LocalEpisode, false), Times.Once());
        }

        [Test]
        public void should_delete_existing_metadata_files_with_the_same_path()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(Builder<EpisodeFile>.CreateListOfSize(1).BuildList());

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, false);

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Delete(It.IsAny<EpisodeFile>(), DeleteMediaFileReason.ManualOverride), Times.Once());
        }
    }
}
