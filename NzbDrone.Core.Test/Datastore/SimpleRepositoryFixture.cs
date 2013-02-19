using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{

    public class SampleType : ModelBase
    {
        public string Name { get; set; }
        public string Tilte { get; set; }
        public string Address { get; set; }
    }

    [TestFixture]
    public class SimpleRepositoryFixture : ObjectDbTest<BasicRepository<SampleType>>
    {
        private SampleType sampleType;


        [SetUp]
        public void Setup()
        {
            WithObjectDb();
            sampleType = Builder<SampleType>
                .CreateNew()
                .With(c => c.OID = 0)
                .Build();

        }

        [Test]
        public void should_be_able_to_add()
        {
            Subject.Insert(sampleType);
            Subject.All().Should().HaveCount(1);
        }



        [Test]
        public void should_be_able_to_delete_model()
        {
            Subject.Insert(sampleType);
            Subject.All().Should().HaveCount(1);

            Subject.Delete(sampleType.OID);
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_find_by_id()
        {
            Subject.Insert(sampleType);
            Subject.Get(sampleType.OID)
                .ShouldHave()
                .AllProperties()
                .EqualTo(sampleType);
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
    }
}