using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using Workarr.Blocklisting;
using Workarr.Housekeeping.Housekeepers;
using Workarr.Languages;
using Workarr.Qualities;
using Workarr.Tv;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBlocklistFixture : DbTest<CleanupOrphanedBlocklist, Blocklist>
    {
        [Test]
        public void should_delete_orphaned_blocklist_items()
        {
            var blocklist = Builder<Blocklist>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.EpisodeIds = new List<int>())
                .With(h => h.Quality = new QualityModel())
                .BuildNew();

            Db.Insert(blocklist);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_blocklist_items()
        {
            var series = Builder<Series>.CreateNew().BuildNew();

            Db.Insert(series);

            var blocklist = Builder<Blocklist>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.EpisodeIds = new List<int>())
                .With(h => h.Quality = new QualityModel())
                .With(b => b.SeriesId = series.Id)
                .BuildNew();

            Db.Insert(blocklist);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
