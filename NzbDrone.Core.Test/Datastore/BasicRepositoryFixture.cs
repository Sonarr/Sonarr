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
        BasicRepositoryFixture : DbTest<BasicRepository<JobDefinition>, JobDefinition>
    {
        private JobDefinition _basicType;


        [SetUp]
        public void Setup()
        {
            _basicType = Builder<JobDefinition>
                    .CreateNew()
                    .With(c => c.Id = 0)
                    .Build();
        }

        [Test]
        public void should_be_able_to_add()
        {
            Subject.Insert(_basicType);
            Subject.All().Should().HaveCount(1);
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
            Subject.Get(_basicType.Id)
                   .ShouldHave()
                   .AllProperties()
                   .EqualTo(_basicType);
        }

        [Test]
        public void getting_model_with_invalid_id_should_throw()
        {
            Assert.Throws<InvalidOperationException>(() => Subject.Get(12));
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