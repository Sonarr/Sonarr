using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class update_quality_minmax_sizeFixture : MigrationTest<Core.Datastore.Migration.update_quality_minmax_size>
    {
        [Test]
        public void should_not_fail_if_empty()
        {
            WithTestDb(c =>
            {

            });

            var items = Mocker.Resolve<QualityDefinitionRepository>().All();

            items.Should().HaveCount(0);
        }

        [Test]
        public void should_set_rawhd_to_null()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("QualityDefinitions").Row(new
                {
                    Quality = 1,
                    Title = "SDTV",
                    MinSize = 0,
                    MaxSize = 100
                })
                .Row(new
                {
                    Quality = 10,
                    Title = "RawHD",
                    MinSize = 0,
                    MaxSize = 100
                });
            });

            var items = Mocker.Resolve<QualityDefinitionRepository>().All();

            items.Should().HaveCount(2);

            items.First(v => v.Quality.Id == 10).MaxSize.Should().NotHaveValue();
        }

        [Test]
        public void should_set_zero_maxsize_to_null()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("QualityDefinitions").Row(new
                {
                    Quality = 1,
                    Title = "SDTV",
                    MinSize = 0,
                    MaxSize = 0
                });
            });

            var items = Mocker.Resolve<QualityDefinitionRepository>().All();

            items.Should().HaveCount(1);

            items.First(v => v.Quality.Id == 1).MaxSize.Should().NotHaveValue();
        }

        [Test]
        public void should_preserve_values()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("QualityDefinitions").Row(new
                {
                    Quality = 1,
                    Title = "SDTV",
                    MinSize = 0,
                    MaxSize = 100
                })
                .Row(new
                {
                    Quality = 10,
                    Title = "RawHD",
                    MinSize = 0,
                    MaxSize = 100
                });
            });

            var items = Mocker.Resolve<QualityDefinitionRepository>().All();

            items.Should().HaveCount(2);

            items.First(v => v.Quality.Id == 1).MaxSize.Should().Be(100);
        }
    }
}
