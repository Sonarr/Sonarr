using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Blacklisting
{
    [TestFixture]
    public class BlacklistRepositoryFixture : DbTest<BlacklistRepository, Blacklist>
    {
        private Blacklist _blacklist;

        [SetUp]
        public void Setup()
        {
            _blacklist = new Blacklist
                     {
                         SeriesId = 12345,
                         EpisodeIds = new List<int> {1},
                         Quality = new QualityModel(Quality.Bluray720p),
                         SourceTitle = "series.title.s01e01",
                         Date = DateTime.UtcNow
                     };
        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Subject.Insert(_blacklist);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_should_have_episode_ids()
        {
            Subject.Insert(_blacklist);

            Subject.All().First().EpisodeIds.Should().Contain(_blacklist.EpisodeIds);
        }

        [Test]
        public void should_check_for_blacklisted_title_case_insensative()
        {
            Subject.Insert(_blacklist);

            Subject.Blacklisted(_blacklist.SourceTitle.ToUpperInvariant()).Should().BeTrue();
        }
    }
}
