using System;
using System.Data;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data.Mapping;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    public class BasicType : ModelBase
    {
        public string Name { get; set; }
        public string Tilte { get; set; }
        public string Address { get; set; }
    }

    [TestFixture]
    public class 
        BasicRepositoryFixture : DbTest<BasicRepository<BasicType>, BasicType>
    {
        private BasicType _basicType;


        [SetUp]
        public void Setup()
        {
            _basicType = Builder<BasicType>
                    .CreateNew()
                    .With(c => c.Id = 0)
                    .Build();

            var mapping = new FluentMappings(true);

            mapping.Entity<BasicType>()
                   .Columns.AutoMapSimpleTypeProperties()
                   .For(c => c.Id).SetAutoIncrement()
                   .SetPrimaryKey();

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