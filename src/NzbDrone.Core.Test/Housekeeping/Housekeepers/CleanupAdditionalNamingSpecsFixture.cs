using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupAdditionalNamingSpecsFixture : DbTest<CleanupAdditionalNamingSpecs, NamingConfig>
    {
        [Test]
        public async Task should_delete_additional_naming_configs()
        {
            var specs = Builder<NamingConfig>.CreateListOfSize(5)
                                             .BuildListOfNew();

            await Db.InsertManyAsync(specs);

            Subject.Clean();
            var namingConfigs = await GetAllStoredModelsAsync();
            namingConfigs.Should().HaveCount(1);
        }

        [Test]
        public async Task should_not_delete_if_only_one_spec()
        {
            var spec = Builder<NamingConfig>.CreateNew()
                                            .BuildNew();

            await Db.InsertAsync(spec);

            Subject.Clean();
            var namingConfigs = await GetAllStoredModelsAsync();
            namingConfigs.Should().HaveCount(1);
        }
    }
}
