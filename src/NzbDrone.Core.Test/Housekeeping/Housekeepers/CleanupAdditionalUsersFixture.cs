using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupAdditionalUsersFixture : DbTest<CleanupAdditionalUsers, User>
    {
        [Test]
        public async Task should_delete_additional_users()
        {
            var specs = Builder<User>.CreateListOfSize(5)
                                             .BuildListOfNew();

            await Db.InsertManyAsync(specs);

            Subject.Clean();
            var users = await GetAllStoredModelsAsync();
            users.Should().HaveCount(1);
        }

        [Test]
        public async Task should_not_delete_if_only_one_user()
        {
            var spec = Builder<User>.CreateNew()
                                            .BuildNew();

            await Db.InsertAsync(spec);

            Subject.Clean();
            var users = await GetAllStoredModelsAsync();
            users.Should().HaveCount(1);
        }
    }
}
