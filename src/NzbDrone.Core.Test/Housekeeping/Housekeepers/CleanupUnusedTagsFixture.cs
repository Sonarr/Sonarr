using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupUnusedTagsFixture : DbTest<CleanupUnusedTags, Tag>
    {
        [Test]
        public void should_delete_unused_tags()
        {
            var tags = Builder<Tag>
                .CreateListOfSize(2)
                .All()
                .With(x => x.Id = 0)
                .BuildList();

            Db.InsertMany(tags);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_used_tags()
        {
            var tags = Builder<Tag>
                .CreateListOfSize(2)
                .All()
                .With(x => x.Id = 0)
                .BuildList();
            Db.InsertMany(tags);

            var restrictions = Builder<ReleaseProfile>.CreateListOfSize(2)
                .All()
                .With(x => x.Id = 0)
                .With(v => v.Tags.Add(tags[0].Id))
                .BuildList();
            Db.InsertMany(restrictions);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
