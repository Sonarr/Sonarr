using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class RenameEpisodeFileServiceFixture : CoreTest<RenameEpisodeFileService>
    {
        private Series _series;
        private List<EpisodeFile> _episodeFiles;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\Series Title".AsOsAgnostic())
                                     .Build();

            _episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                                                .All()
                                                .With(e => e.SeriesId = _series.Id)
                                                .With(e => e.SeasonNumber = 1)
                                                .Build()
                                                .ToList();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(_series.Id))
                  .Returns(_series);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesByFileId(It.IsAny<int>()))
                  .Returns(new List<Episode>());

            // By default every file renames to a unique, uncontested path so tests that don't care about
            // naming conflicts don't need to stub the builder themselves.
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(f => f.BuildFilePath(It.IsAny<List<Episode>>(), It.IsAny<Series>(), It.IsAny<EpisodeFile>(), It.IsAny<string>(), It.IsAny<NamingConfig>(), It.IsAny<List<CustomFormat>>()))
                  .Returns<List<Episode>, Series, EpisodeFile, string, NamingConfig, List<CustomFormat>>((episodes, series, file, ext, namingConfig, customFormats) => Path.Combine(series.Path, "Renamed-" + file.RelativePath));
        }

        private void GivenNoEpisodeFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<EpisodeFile>());
        }

        private void GivenEpisodeFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(_episodeFiles);
        }

        private void GivenMovedFiles()
        {
            Mocker.GetMock<IMoveEpisodeFiles>()
                  .Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), _series));
        }

        // Points a specific file's computed destination at the given (series-relative) path, so tests can
        // construct swaps, rotations, and blocked conflicts without touching the real naming logic or disk.
        private void GivenTargetPath(EpisodeFile episodeFile, string targetRelativePath)
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(f => f.BuildFilePath(It.IsAny<List<Episode>>(), _series, episodeFile, It.IsAny<string>(), It.IsAny<NamingConfig>(), It.IsAny<List<CustomFormat>>()))
                  .Returns(Path.Combine(_series.Path, targetRelativePath));
        }

        [Test]
        public void should_not_publish_event_if_no_files_to_rename()
        {
            GivenNoEpisodeFiles();

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_not_publish_event_if_no_files_are_renamed()
        {
            GivenEpisodeFiles();

            // Both files already sit at their correctly-computed path - nothing to do.
            GivenTargetPath(_episodeFiles[0], _episodeFiles[0].RelativePath);
            GivenTargetPath(_episodeFiles[1], _episodeFiles[1].RelativePath);

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1, 2 }));

            Mocker.GetMock<IMoveEpisodeFiles>()
                  .Verify(v => v.MoveEpisodeFile(It.IsAny<EpisodeFile>(), It.IsAny<Series>()), Times.Never());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_publish_event_if_files_are_renamed()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Once());
        }

        [Test]
        public void should_update_moved_files()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1 }));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_get_episodefiles_by_ids_only()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            var files = new List<int> { 1 };

            Subject.Execute(new RenameFilesCommand(_series.Id, files));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Get(files), Times.Once());
        }

        [Test]
        public void should_rename_swapped_episode_files()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            // Episode 1 and episode 2 trade filenames - a closed, two-file rotation.
            GivenTargetPath(_episodeFiles[0], _episodeFiles[1].RelativePath);
            GivenTargetPath(_episodeFiles[1], _episodeFiles[0].RelativePath);

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1, 2 }));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(2));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Once());

            // Exactly one file needs to be displaced to a temporary name to break the rotation.
            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_rename_episode_files_in_a_three_way_rotation()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(3)
                                                    .All()
                                                    .With(e => e.SeriesId = _series.Id)
                                                    .With(e => e.SeasonNumber = 1)
                                                    .Build()
                                                    .ToList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(episodeFiles);

            GivenMovedFiles();

            // 1 -> 2's spot, 2 -> 3's spot, 3 -> 1's spot: episodes renumbered E1->E2->E3->E1.
            GivenTargetPath(episodeFiles[0], episodeFiles[1].RelativePath);
            GivenTargetPath(episodeFiles[1], episodeFiles[2].RelativePath);
            GivenTargetPath(episodeFiles[2], episodeFiles[0].RelativePath);

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1, 2, 3 }));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(3));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Once());

            // Exactly one file needs to be displaced to a temporary name to break the rotation.
            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_still_rename_files_unaffected_by_a_blocked_conflict()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            const string blockedTargetRelativePath = "Blocked/Target.mkv";

            // File 0's target is permanently occupied by something outside this batch.
            GivenTargetPath(_episodeFiles[0], blockedTargetRelativePath);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(d => d.FileExists(Path.Combine(_series.Path, blockedTargetRelativePath)))
                  .Returns(true);

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1, 2 }));

            // The blocked file is never even attempted...
            Mocker.GetMock<IMoveEpisodeFiles>()
                  .Verify(v => v.MoveEpisodeFile(_episodeFiles[0], _series), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(_episodeFiles[0]), Times.Never());

            // ...but the unrelated, independent file still renames normally.
            Mocker.GetMock<IMoveEpisodeFiles>()
                  .Verify(v => v.MoveEpisodeFile(_episodeFiles[1], _series), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(_episodeFiles[1]), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_move_any_file_in_a_chain_that_depends_on_a_blocked_file()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            const string blockedTargetRelativePath = "Blocked/Target.mkv";

            // File 0 can only move once file 1 vacates its current location...
            GivenTargetPath(_episodeFiles[0], _episodeFiles[1].RelativePath);

            // ...but file 1's own target is permanently occupied by something outside this batch, so it can
            // never vacate its spot - meaning file 0 can never safely move into it either.
            GivenTargetPath(_episodeFiles[1], blockedTargetRelativePath);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(d => d.FileExists(Path.Combine(_series.Path, blockedTargetRelativePath)))
                  .Returns(true);

            Subject.Execute(new RenameFilesCommand(_series.Id, new List<int> { 1, 2 }));

            Mocker.GetMock<IMoveEpisodeFiles>()
                  .Verify(v => v.MoveEpisodeFile(It.IsAny<EpisodeFile>(), It.IsAny<Series>()), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Never());

            // Nothing was ever moved, so there is no temporary state to create in the first place.
            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());

            ExceptionVerification.ExpectedWarns(2);
        }
    }
}
