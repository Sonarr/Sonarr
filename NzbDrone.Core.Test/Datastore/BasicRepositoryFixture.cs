using System;
using System.Data;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using ServiceStack.OrmLite;

namespace NzbDrone.Core.Test.Datastore
{
    public class BaiscType : ModelBase
    {
        public string Name { get; set; }
        public string Tilte { get; set; }
        public string Address { get; set; }
    }

    [TestFixture]
    public class BasicRepositoryFixture : DbTest<BasicRepository<BaiscType>,BaiscType>
    {
        private BaiscType _baiscType;


        [SetUp]
        public void Setup()
        {
            _baiscType = Builder<BaiscType>
                    .CreateNew()
                    .With(c => c.Id = 0)
                    .Build();

            Mocker.Resolve<IDbConnection>().CreateTable<BaiscType>();
        }

        [Test]
        public void should_be_able_to_add()
        {
            Subject.Insert(_baiscType);
            Subject.All().Should().HaveCount(1);
        }



        [Test]
        public void should_be_able_to_delete_model()
        {
            Subject.Insert(_baiscType);
            Subject.All().Should().HaveCount(1);

            Subject.Delete(_baiscType.Id);
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_find_by_id()
        {
            Subject.Insert(_baiscType);
            Subject.Get(_baiscType.Id)
                   .ShouldHave()
                   .AllProperties()
                   .EqualTo(_baiscType);
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