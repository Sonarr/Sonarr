using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
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

        [SetUp]
        public void Setup()
        {
            _rejectedDecisions = new List<ImportDecision>();
            _approvedDecisions = new List<ImportDecision>();

            var series = Builder<Series>.CreateNew()
                                        .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                                           .Build();

            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), "Rejected!"));
            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), "Rejected!"));
            _rejectedDecisions.Add(new ImportDecision(new LocalEpisode(), "Rejected!"));

            foreach (var episode in episodes)
            {
                _approvedDecisions.Add(new ImportDecision
                                           (
                                           new LocalEpisode
                                               {
                                                   Series = series,
                                                   Episodes = new List<Episode> {episode},
                                                   Path = @"C:\Test\TV\30 Rock\30 Rock - S01E01 - Pilit.avi".AsOsAgnostic(),
                                                   Quality = new QualityModel(Quality.Bluray720p)

                                               }));
            }

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Setup(s => s.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), It.IsAny<LocalEpisode>()));
        }
            
        [Test]
        public void should_return_empty_list_if_there_are_no_approved_decisions()
        {
            Subject.Import(_rejectedDecisions).Should().BeEmpty();
        }

        [Test]
        public void should_import_each_approved()
        {
            Subject.Import(_approvedDecisions).Should().HaveCount(5);
        }

        [Test]
        public void should_only_import_approved()
        {
            var all = new List<ImportDecision>();
            all.AddRange(_rejectedDecisions);
            all.AddRange(_approvedDecisions);

            Subject.Import(all).Should().HaveCount(5);
        }

        [Test]
        public void should_only_import_each_episode_once()
        {
            var all = new List<ImportDecision>();
            all.AddRange(_approvedDecisions);
            all.Add(new ImportDecision(_approvedDecisions.First().LocalEpisode));

            Subject.Import(all).Should().HaveCount(5);
        }

        [Test]
        public void should_move_new_downloads()
        {
            Subject.Import(new List<ImportDecision> {_approvedDecisions.First()}, true);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), _approvedDecisions.First().LocalEpisode),
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
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() });

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeEpisodeFile(It.IsAny<EpisodeFile>(), _approvedDecisions.First().LocalEpisode),
                          Times.Never());
        }

        [Test]
        public void should_not_trigger_EpisodeImportedEvent_for_existing_files()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() });

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<EpisodeImportedEvent>()), Times.Never());
        }
    }
}