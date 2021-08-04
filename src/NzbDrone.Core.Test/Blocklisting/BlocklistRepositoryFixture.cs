using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Blocklisting
{
    [TestFixture]
    public class BlocklistRepositoryFixture : DbTest<BlocklistRepository, Blocklist>
    {
        private Blocklist _blocklist;

        [SetUp]
        public void Setup()
        {
            _blocklist = new Blocklist
                     {
                         SeriesId = 12345,
                         EpisodeIds = new List<int> { 1 },
                         Quality = new QualityModel(Quality.Bluray720p),
                         Language = Core.Languages.Language.English,
                         SourceTitle = "series.title.s01e01",
                         Date = DateTime.UtcNow
                     };
        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Subject.Insert(_blocklist);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_should_have_episode_ids()
        {
            Subject.Insert(_blocklist);

            Subject.All().First().EpisodeIds.Should().Contain(_blocklist.EpisodeIds);
        }

        [Test]
        public void should_check_for_blocklisted_title_case_insensative()
        {
            Subject.Insert(_blocklist);

            Subject.BlocklistedByTitle(_blocklist.SeriesId, _blocklist.SourceTitle.ToUpperInvariant()).Should().HaveCount(1);
        }
    }
}
