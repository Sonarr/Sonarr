using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityProfileRankServiceFixture : CoreTest<QualityProfileRankService>
    {
        private QualityProfile BuildProfile(int id, params Quality[] items)
        {
            return new QualityProfile
            {
                Id = id,
                Items = items.Select(q => new QualityProfileQualityItem { Quality = q, Allowed = true }).ToList()
            };
        }

        [Test]
        public void should_score_single_item_profile_as_1()
        {
            var profile = BuildProfile(7, Quality.SDTV);

            var ranks = Subject.ComputeRanks(profile).ToList();

            ranks.Should().HaveCount(1);
            ranks[0].ProfileId.Should().Be(7);
            ranks[0].QualityId.Should().Be(Quality.SDTV.Id);
            ranks[0].Score.Should().Be(1.0);
        }

        [Test]
        public void should_score_first_item_as_0_and_last_as_1()
        {
            var profile = BuildProfile(3, Quality.SDTV, Quality.HDTV720p, Quality.Bluray1080p);

            var ranks = Subject.ComputeRanks(profile)
                .ToDictionary(r => r.QualityId, r => r.Score);

            ranks[Quality.SDTV.Id].Should().Be(0.0);
            ranks[Quality.HDTV720p.Id].Should().Be(0.5);
            ranks[Quality.Bluray1080p.Id].Should().Be(1.0);
        }

        [Test]
        public void should_flatten_grouped_items_and_share_group_index()
        {
            var profile = new QualityProfile
            {
                Id = 4,
                Items = new List<QualityProfileQualityItem>
                {
                    new() { Quality = Quality.SDTV, Allowed = true },
                    new()
                    {
                        Id = 1000,
                        Name = "HD",
                        Allowed = true,
                        Items = new List<QualityProfileQualityItem>
                        {
                            new() { Quality = Quality.HDTV720p, Allowed = true },
                            new() { Quality = Quality.WEBDL720p, Allowed = true }
                        }
                    },
                    new() { Quality = Quality.Bluray1080p, Allowed = true }
                }
            };

            var ranks = Subject.ComputeRanks(profile)
                .ToDictionary(r => r.QualityId, r => r.Score);

            ranks[Quality.SDTV.Id].Should().Be(0.0);
            ranks[Quality.HDTV720p.Id].Should().Be(0.5);
            ranks[Quality.WEBDL720p.Id].Should().Be(0.5);
            ranks[Quality.Bluray1080p.Id].Should().Be(1.0);
        }

        [Test]
        public void should_emit_ranks_for_disallowed_items_too()
        {
            var profile = BuildProfile(9, Quality.SDTV, Quality.HDTV720p);
            profile.Items[0].Allowed = false;

            var ranks = Subject.ComputeRanks(profile).ToList();

            ranks.Should().HaveCount(2);
        }

        [Test]
        public void seed_should_upsert_ranks_for_every_profile()
        {
            var p1 = BuildProfile(1, Quality.SDTV, Quality.HDTV720p);
            var p2 = BuildProfile(2, Quality.Bluray1080p);

            Subject.SeedAll(new List<QualityProfile> { p1, p2 });

            Mocker.GetMock<IQualityProfileRankRepository>()
                  .Verify(x => x.ReplaceForProfile(1, It.Is<IEnumerable<QualityProfileQualityRank>>(r => r.Count() == 2)), Times.Once);
            Mocker.GetMock<IQualityProfileRankRepository>()
                  .Verify(x => x.ReplaceForProfile(2, It.Is<IEnumerable<QualityProfileQualityRank>>(r => r.Count() == 1)), Times.Once);
        }
    }
}
