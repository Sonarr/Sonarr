using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBlocklistFixture : DbTest<CleanupOrphanedBlocklist, Blocklist>
    {
        [Test]
        public async Task should_delete_orphaned_blocklist_items()
        {
            var blocklist = Builder<Blocklist>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.EpisodeIds = new List<int>())
                .With(h => h.Quality = new QualityModel())
                .BuildNew();

            await Db.InsertAsync(blocklist);
            Subject.Clean();
            var blocklists = await GetAllStoredModelsAsync();
            blocklists.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_unorphaned_blocklist_items()
        {
            var series = Builder<Series>.CreateNew().BuildNew();

            await Db.InsertAsync(series);

            var blocklist = Builder<Blocklist>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.EpisodeIds = new List<int>())
                .With(h => h.Quality = new QualityModel())
                .With(b => b.SeriesId = series.Id)
                .BuildNew();

            await Db.InsertAsync(blocklist);

            Subject.Clean();
            var blocklists = await GetAllStoredModelsAsync();
            blocklists.Should().HaveCount(1);
        }
    }
}
