using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityModelComparerFixture : CoreTest
    {
        public QualityModelComparer Subject { get; set; }

        private void GivenDefaultQualityProfile()
        {
            Subject = new QualityModelComparer(new QualityProfile { Items = QualityFixture.GetDefaultQualities() });
        }

        private void GivenCustomQualityProfile()
        {
            Subject = new QualityModelComparer(new QualityProfile { Items = QualityFixture.GetDefaultQualities(Quality.Bluray720p, Quality.DVD) });
        }

        [Test]
        public void Icomparer_greater_test()
        {
            GivenDefaultQualityProfile();

            var first = new QualityModel(Quality.DVD, true);
            var second = new QualityModel(Quality.Bluray1080p, true);

            var compare = Subject.Compare(second, first);

            compare.Should().BeGreaterThan(0);
        }

        [Test]
        public void Icomparer_greater_proper()
        {
            GivenDefaultQualityProfile();

            var first = new QualityModel(Quality.Bluray1080p, false);
            var second = new QualityModel(Quality.Bluray1080p, true);

            var compare = Subject.Compare(second, first);

            compare.Should().BeGreaterThan(0);
        }

        [Test]
        public void Icomparer_lesser()
        {
            GivenDefaultQualityProfile();

            var first = new QualityModel(Quality.DVD, true);
            var second = new QualityModel(Quality.Bluray1080p, true);

            var compare = Subject.Compare(first, second);

            compare.Should().BeLessThan(0);
        }

        [Test]
        public void Icomparer_lesser_proper()
        {
            GivenDefaultQualityProfile();

            var first = new QualityModel(Quality.DVD, false);
            var second = new QualityModel(Quality.DVD, true);

            var compare = Subject.Compare(first, second);

            compare.Should().BeLessThan(0);
        }

        [Test]
        public void Icomparer_greater_custom_order()
        {
            GivenCustomQualityProfile();

            var first = new QualityModel(Quality.DVD, true);
            var second = new QualityModel(Quality.Bluray720p, true);

            var compare = Subject.Compare(first, second);

            compare.Should().BeGreaterThan(0);
        }
    }
}
