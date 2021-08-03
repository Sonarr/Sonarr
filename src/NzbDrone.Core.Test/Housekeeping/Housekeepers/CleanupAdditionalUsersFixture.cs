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
        public void should_delete_additional_users()
        {
            var specs = Builder<User>.CreateListOfSize(5)
                                             .BuildListOfNew();

            Db.InsertMany(specs);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_not_delete_if_only_one_user()
        {
            var spec = Builder<User>.CreateNew()
                                            .BuildNew();

            Db.Insert(spec);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
