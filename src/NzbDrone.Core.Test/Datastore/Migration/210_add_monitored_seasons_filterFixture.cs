using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_monitored_seasons_filterFixture : MigrationTest<add_monitored_seasons_filter>
    {
        [Test]
        public void equal_both_becomes_equal_every_option()
        {
            var filter = new FilterSettings210
            {
                key = "hasUnmonitoredSeason",
                value = new List<object> { true, false },
                type = "equal"
            };

            var filtersJson = new List<FilterSettings210> { filter };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("seasonsMonitoredStatus");
            filters[0].value.Should().BeEquivalentTo(new List<object> { "all", "partial", "none" });
            filters[0].type.Should().Be("equal");
        }

        [Test]
        public void notEqual_both_becomes_notEqual_every_option()
        {
            var filter = new FilterSettings210
            {
                key = "hasUnmonitoredSeason",
                value = new List<object> { true, false },
                type = "notEqual"
            };

            var filtersJson = new List<FilterSettings210> { filter };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("seasonsMonitoredStatus");
            filters[0].value.Should().BeEquivalentTo(new List<object> { "all", "partial", "none" });
            filters[0].type.Should().Be("notEqual");
        }

        [Test]
        public void equal_true_becomes_notEqual_all()
        {
            var filter = new FilterSettings210
            {
                key = "hasUnmonitoredSeason",
                value = new List<object> { true },
                type = "equal"
            };

            var filtersJson = new List<FilterSettings210> { filter };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("seasonsMonitoredStatus");
            filters[0].value.Should().BeEquivalentTo(new List<object> { "all" });
            filters[0].type.Should().Be("notEqual");
        }

        [Test]
        public void notEqual_true_becomes_equal_all()
        {
            var filter = new FilterSettings210
            {
                key = "hasUnmonitoredSeason",
                value = new List<object> { true },
                type = "notEqual"
            };

            var filtersJson = new List<FilterSettings210> { filter };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("seasonsMonitoredStatus");
            filters[0].value.Should().BeEquivalentTo(new List<object> { "all" });
            filters[0].type.Should().Be("equal");
        }

        [Test]
        public void equal_false_becomes_equal_all()
        {
            var filter = new FilterSettings210
            {
                key = "hasUnmonitoredSeason",
                value = new List<object> { false },
                type = "equal"
            };

            var filtersJson = new List<FilterSettings210> { filter };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("seasonsMonitoredStatus");
            filters[0].value.Should().BeEquivalentTo(new List<object> { "all" });
            filters[0].type.Should().Be("equal");
        }

        [Test]
        public void notEqual_false_becomes_notEqual_all()
        {
            var filter = new FilterSettings210
            {
                key = "hasUnmonitoredSeason",
                value = new List<object> { false },
                type = "notEqual"
            };

            var filtersJson = new List<FilterSettings210> { filter };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("seasonsMonitoredStatus");
            filters[0].value.Should().BeEquivalentTo(new List<object> { "all" });
            filters[0].type.Should().Be("notEqual");
        }

        [Test]
        public void missing_hasUnmonitored_unchanged()
        {
            var filter = new FilterSettings210
            {
                key = "monitored",
                value = new List<object> { false },
                type = "equal"
            };

            var filtersJson = new List<FilterSettings210> { filter };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("monitored");
            filters[0].value.Should().BeEquivalentTo(new List<object> { false });
            filters[0].type.Should().Be("equal");
        }

        [Test]
        public void has_hasUnmonitored_not_in_first_entry()
        {
            var filter1 = new FilterSettings210
            {
                key = "monitored",
                value = new List<object> { false },
                type = "equal"
            };
            var filter2 = new FilterSettings210
            {
                key = "hasUnmonitoredSeason",
                value = new List<object> { true },
                type = "equal"
            };

            var filtersJson = new List<FilterSettings210> { filter1, filter2 };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("monitored");
            filters[0].value.Should().BeEquivalentTo(new List<object> { false });
            filters[0].type.Should().Be("equal");
            filters[1].key.Should().Be("seasonsMonitoredStatus");
            filters[1].value.Should().BeEquivalentTo(new List<object> { "all" });
            filters[1].type.Should().Be("notEqual");
        }

        [Test]
        public void has_umonitored_is_empty()
        {
            var filter = new FilterSettings210
            {
                key = "hasUnmonitoredSeason",
                value = new List<object> {  },
                type = "equal"
            };

            var filtersJson = new List<FilterSettings210> { filter };

            var filtersString = filtersJson.ToJson();

            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("CustomFilters").Row(new
                {
                    Id = 1,
                    Type = "series",
                    Label = "Is Both",
                    Filters = filtersString
                });
            });

            var items = db.Query<FilterDefinition210>("SELECT * FROM \"CustomFilters\"");

            items.Should().HaveCount(1);
            items.First().Type.Should().Be("series");
            items.First().Label.Should().Be("Is Both");

            var filters = JsonConvert.DeserializeObject<List<FilterSettings210>>(items.First().Filters);
            filters[0].key.Should().Be("seasonsMonitoredStatus");
            filters[0].value.Should().BeEquivalentTo(new List<object> {  });
            filters[0].type.Should().Be("equal");
        }
    }

    public class FilterDefinition210
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Filters { get; set; }
    }

    public class FilterSettings210
    {
        public string key { get; set; }
        public List<object> value { get; set; }
        public string type { get; set; }
    }
}
