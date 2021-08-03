using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class
        BasicRepositoryFixture : DbTest<BasicRepository<ScheduledTask>, ScheduledTask>
    {
        private ScheduledTask _basicType;

        [SetUp]
        public void Setup()
        {
            _basicType = Builder<ScheduledTask>
                    .CreateNew()
                    .With(c => c.Id = 0)
                    .With(c => c.LastExecution = DateTime.UtcNow)
                    .Build();
        }

        [Test]
        public void should_be_able_to_add()
        {
            Subject.Insert(_basicType);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void purge_should_delete_all()
        {
            Subject.InsertMany(Builder<ScheduledTask>.CreateListOfSize(10).BuildListOfNew());

            AllStoredModels.Should().HaveCount(10);

            Subject.Purge();

            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_delete_model()
        {
            Subject.Insert(_basicType);
            Subject.All().Should().HaveCount(1);

            Subject.Delete(_basicType.Id);
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_find_by_id()
        {
            Subject.Insert(_basicType);
            var storeObject = Subject.Get(_basicType.Id);

            storeObject.Should().BeEquivalentTo(_basicType, o => o.IncludingAllRuntimeProperties());
        }

        [Test]
        public void should_be_able_to_get_single()
        {
            Subject.Insert(_basicType);
            Subject.SingleOrDefault().Should().NotBeNull();
        }

        [Test]
        public void single_or_default_on_empty_table_should_return_null()
        {
            Subject.SingleOrDefault().Should().BeNull();
        }

        [Test]
        public void getting_model_with_invalid_id_should_throw()
        {
            Assert.Throws<ModelNotFoundException>(() => Subject.Get(12));
        }

        [Test]
        public void get_all_with_empty_db_should_return_empty_list()
        {
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_call_ToList_on_empty_quariable()
        {
            Subject.All().ToList().Should().BeEmpty();
        }
    }
}
