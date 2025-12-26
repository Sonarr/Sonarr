using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.AutoTagging.Specifications;
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
        public async Task should_delete_unused_tags()
        {
            var tags = Builder<Tag>
                .CreateListOfSize(2)
                .All()
                .With(x => x.Id = 0)
                .BuildList();

            await Db.InsertManyAsync(tags);
            Subject.Clean();
            var loadedTags = await GetAllStoredModelsAsync();
            loadedTags.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_used_tags()
        {
            var tags = Builder<Tag>
                .CreateListOfSize(2)
                .All()
                .With(x => x.Id = 0)
                .BuildList();
            await Db.InsertManyAsync(tags);

            var restrictions = Builder<ReleaseProfile>.CreateListOfSize(2)
                .All()
                .With(x => x.Id = 0)
                .With(v => v.Tags.Add(tags[0].Id))
                .BuildList();
            await Db.InsertManyAsync(restrictions);

            Subject.Clean();
            var loadedTags = await GetAllStoredModelsAsync();
            loadedTags.Should().HaveCount(1);
        }

        [Test]
        public async Task should_not_delete_used_auto_tagging_tag_specification_tags()
        {
            var tags = Builder<Tag>
                .CreateListOfSize(2)
                .All()
                .With(x => x.Id = 0)
                .BuildList();
            await Db.InsertManyAsync(tags);

            var autoTags = Builder<AutoTag>.CreateListOfSize(1)
                .All()
                .With(x => x.Id = 0)
                .With(x => x.Specifications = new List<IAutoTaggingSpecification>
                {
                    new TagSpecification
                    {
                        Name = "Test",
                        Value = tags[0].Id
                    }
                })
                .BuildList();

            Mocker.GetMock<IAutoTaggingRepository>().Setup(s => s.AllAsync())
                .ReturnsAsync(autoTags);

            Subject.Clean();
            var loadedTags = await GetAllStoredModelsAsync();
            loadedTags.Should().HaveCount(1);
        }
    }
}
