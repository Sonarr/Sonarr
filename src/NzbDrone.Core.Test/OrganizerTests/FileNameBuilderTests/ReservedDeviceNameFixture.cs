using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class ReservedDeviceNameFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode1;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episode1 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [TestCase("Con Game", "Con_Game")]
        [TestCase("Com1 Sat", "Com1_Sat")]
        public void should_replace_reserved_device_name_in_series_folder(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.SeriesFolderFormat = "{Series.Title}";

            Subject.GetSeriesFolder(_series).Should().Be($"{expected}");
        }

        [TestCase("Con Game", "Con_Game")]
        [TestCase("Com1 Sat", "Com1_Sat")]
        public void should_replace_reserved_device_name_in_season_folder(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.SeasonFolderFormat = "{Series.Title} - Season {Season:00}";

            Subject.GetSeasonFolder(_series, 1).Should().Be($"{expected} - Season 01");
        }

        [TestCase("Con Game", "Con_Game")]
        [TestCase("Com1 Sat", "Com1_Sat")]
        public void should_replace_reserved_device_name_in_file_name(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.StandardEpisodeFormat = "{Series.Title} - S{Season:00}E{Episode:00}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile).Should().Be($"{expected} - S15E06");
        }
    }
}
