using System.Collections.Generic;
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

        private void GivenGroupedProfile()
        {
            var profile = new Profile
                          {
                              Items = new List<ProfileQualityItem>
                                      {
                                          new ProfileQualityItem
                                          {
                                              Allowed = false,
                                              Quality = Quality.SDTV
                                          },
                                          new ProfileQualityItem
                                          {
                                              Allowed = false,
                                              Quality = Quality.DVD
                                          },
                                          new ProfileQualityItem
                                          {
                                              Allowed = true,
                                              Items = new List<ProfileQualityItem>
                                                      {
                                                          new ProfileQualityItem
                                                          {
                                                              Allowed = true,
                                                              Quality = Quality.HDTV720p
                                                          },
                                                          new ProfileQualityItem
                                                          {
                                                              Allowed = true,
                                                              Quality = Quality.WEBDL720p
                                                          }
                                                      }
                                          },
                                          new ProfileQualityItem
                                          {
                                              Allowed = true,
                                              Quality = Quality.Bluray720p
                                          }
                                      }
                          };

            Subject = new QualityModelComparer(profile);
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

        [Test]
        public void should_ignore_group_order_by_default()
        {
            GivenGroupedProfile();

            var first = new QualityModel(Quality.HDTV720p);
            var second = new QualityModel(Quality.WEBDL720p);

            var compare = Subject.Compare(first, second);

            compare.Should().Be(0);
        }

        [Test]
        public void should_respect_group_order()
        {
            GivenGroupedProfile();

            var first = new QualityModel(Quality.HDTV720p);
            var second = new QualityModel(Quality.WEBDL720p);

            var compare = Subject.Compare(first, second, true);

            compare.Should().BeLessThan(0);
        }
    }
}
