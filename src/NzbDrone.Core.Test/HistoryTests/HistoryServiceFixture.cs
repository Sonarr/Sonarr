using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using System.Collections.Generic;
using NzbDrone.Core.Test.Qualities;
using FluentAssertions;

namespace NzbDrone.Core.Test.HistoryTests
{
    public class HistoryServiceFixture : CoreTest<HistoryService>
    {
        private QualityProfile _profile;
        private QualityProfile _profileCustom;

        [SetUp]
        public void Setup()
        {
            _profile = new QualityProfile { Cutoff = Quality.WEBDL720p, Items = QualityFixture.GetDefaultQualities() };
            _profileCustom = new QualityProfile { Cutoff = Quality.WEBDL720p, Items = QualityFixture.GetDefaultQualities(Quality.DVD) };
        }

        [Test]
        public void should_return_null_if_no_history()
        {
            Mocker.GetMock<IHistoryRepository>()
                .Setup(v => v.GetBestQualityInHistory(2))
                .Returns(new List<QualityModel>());

            var quality = Subject.GetBestQualityInHistory(_profile, 2);

            quality.Should().BeNull();
        }

        [Test]
        public void should_return_best_quality()
        {
            Mocker.GetMock<IHistoryRepository>()
                .Setup(v => v.GetBestQualityInHistory(2))
                .Returns(new List<QualityModel> { new QualityModel(Quality.DVD), new QualityModel(Quality.Bluray1080p) });

            var quality = Subject.GetBestQualityInHistory(_profile, 2);

            quality.Should().Be(new QualityModel(Quality.Bluray1080p));
        }

        [Test]
        public void should_return_best_quality_with_custom_order()
        {
            Mocker.GetMock<IHistoryRepository>()
                .Setup(v => v.GetBestQualityInHistory(2))
                .Returns(new List<QualityModel> { new QualityModel(Quality.DVD), new QualityModel(Quality.Bluray1080p) });

            var quality = Subject.GetBestQualityInHistory(_profileCustom, 2);

            quality.Should().Be(new QualityModel(Quality.DVD));
        }
    }
}