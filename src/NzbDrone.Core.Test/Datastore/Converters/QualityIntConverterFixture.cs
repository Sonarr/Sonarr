using System;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class QualityIntConverterFixture : CoreTest<QualityIntConverter>
    {
        [Test]
        public void should_return_int_when_saving_quality_to_db()
        {
            var quality = Quality.Bluray1080p;

            Subject.ToDB(quality).Should().Be(quality.Id);
        }

        [Test]
        public void should_return_0_when_saving_db_null_to_db()
        {
            Subject.ToDB(DBNull.Value).Should().Be(0);
        }

        [Test]
        public void should_throw_when_saving_another_object_to_db()
        {
            Assert.Throws<InvalidOperationException>(() => Subject.ToDB("Not a quality"));
        }

        [Test]
        public void should_return_quality_when_getting_string_from_db()
        {
            var quality = Quality.Bluray1080p;

            var context = new ConverterContext
                          {
                              DbValue = quality.Id
                          };

            Subject.FromDB(context).Should().Be(quality);
        }

        [Test]
        public void should_return_db_null_for_db_null_value_when_getting_from_db()
        {
            var context = new ConverterContext
                          {
                              DbValue = DBNull.Value
                          };

            Subject.FromDB(context).Should().Be(Quality.Unknown);
        }
    }
}
