using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore.SQLiteMigrationHelperTests
{
    [TestFixture]
    public class DuplicateFixture : DbTest
    {
        private SqLiteMigrationHelper _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = Mocker.Resolve<SqLiteMigrationHelper>();
        }


        [Test]
        public void get_duplicates()
        {
            var series = Builder<Series>.CreateListOfSize(10)
                                        .Random(3)
                                        .With(c => c.QualityProfileId = 100)
                                        .BuildListOfNew();

            Db.InsertMany(series);

            var duplicates = _subject.GetDuplicates<int>("series", "QualityProfileId").ToList();


            duplicates.Should().HaveCount(1);
            duplicates.First().Should().HaveCount(3);
        }

    }
}