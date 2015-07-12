using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityModelComparerFixture : CoreTest
    {
        public QualityModelComparer Subject { get; set; }

        private void GivenDefaultProfile()
        {
            Subject = new QualityModelComparer(new Profile { Items = QualityFixture.GetDefaultQualities() });
        }

        private void GivenCustomProfile()
        {
            Subject = new QualityModelComparer(new Profile { Items = QualityFixture.GetDefaultQualities(Quality.Bluray720p, Quality.DVD) });
        }

        [Test]
        public void should_be_greater_when_first_quality_is_greater_than_second()
        {
            GivenDefaultProfile();

            var first = new QualityModel(Quality.Bluray1080p);
            var second = new QualityModel(Quality.DVD);

            var compare = Subject.Compare(first, second);

            compare.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_lesser_when_second_quality_is_greater_than_first()
        {
            GivenDefaultProfile();

            var first = new QualityModel(Quality.DVD);
            var second = new QualityModel(Quality.Bluray1080p);

            var compare = Subject.Compare(first, second);

            compare.Should().BeLessThan(0);
        }

        [Test]
        public void should_be_greater_when_first_quality_is_a_proper_for_the_same_quality()
        {
            GivenDefaultProfile();

            var first = new QualityModel(Quality.Bluray1080p, new Revision(version: 2));
            var second = new QualityModel(Quality.Bluray1080p, new Revision(version: 1));

            var compare = Subject.Compare(first, second);

            compare.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_greater_when_using_a_custom_profile()
        {
            GivenCustomProfile();

            var first = new QualityModel(Quality.DVD);
            var second = new QualityModel(Quality.Bluray720p);

            var compare = Subject.Compare(first, second);

            compare.Should().BeGreaterThan(0);
        }
    }
}
