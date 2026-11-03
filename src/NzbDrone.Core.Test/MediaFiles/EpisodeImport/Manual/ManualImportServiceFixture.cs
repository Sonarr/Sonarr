using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation;
using NzbDrone.Core.MediaFiles.EpisodeImport.Manual;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Manual
{
    [TestFixture]
    public class ManualImportServiceFixture : CoreTest<ManualImportService>
    {
        private Series _series;
        private string _path;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Path = @"C:\TV\Series Title".AsOsAgnostic())
                .With(s => s.OriginalLanguage = Language.English)
                .Build();

            _path = @"C:\Downloads\Series.Title.S01-S03\Series.Title.E05.mkv".AsOsAgnostic();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(_series.Id))
                  .Returns(_series);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodes(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Episode> { new Episode { Id = 1, SeasonNumber = 2, EpisodeNumber = 5 } });

            Mocker.GetMock<IAggregationService>()
                  .Setup(s => s.Augment(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>()))
                  .Returns<LocalEpisode, DownloadClientItem>((l, d) => l);
        }

        private void GivenRejections(params ImportRejection[] rejections)
        {
            Mocker.GetMock<IMakeImportDecision>()
                  .Setup(s => s.GetDecision(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>()))
                  .Returns<LocalEpisode, DownloadClientItem>((l, d) => new ImportDecision(l, rejections));
        }

        private ManualImportItem Reprocess()
        {
            return Subject.ReprocessItem(_path, null, _series.Id, 2, new List<int> { 1 }, null, new QualityModel(), null, 0, ReleaseType.MultiSeasonPack);
        }

        [Test]
        public void should_drop_multi_season_rejection_when_episodes_were_picked()
        {
            GivenRejections(new ImportRejection(ImportRejectionReason.MultiSeason, "Unable to determine season"));

            Reprocess().Rejections.Should().BeEmpty();
        }

        [Test]
        public void should_keep_other_rejections_when_episodes_were_picked()
        {
            GivenRejections(new ImportRejection(ImportRejectionReason.MultiSeason, "Unable to determine season"),
                            new ImportRejection(ImportRejectionReason.Sample, "Sample"));

            Reprocess().Rejections.Select(r => r.Reason).Should().Equal(ImportRejectionReason.Sample);
        }
    }
}
