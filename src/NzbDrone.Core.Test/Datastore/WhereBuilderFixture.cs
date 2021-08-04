using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class WhereBuilderFixture : CoreTest
    {
        private WhereBuilder _subject;

        [OneTimeSetUp]
        public void MapTables()
        {
            // Generate table mapping
            Mocker.Resolve<DbFactory>();
        }

        private WhereBuilder Where(Expression<Func<Series, bool>> filter)
        {
            return new WhereBuilder(filter, true, 0);
        }

        [Test]
        public void where_equal_const()
        {
            _subject = Where(x => x.Id == 10);

            _subject.ToString().Should().Be($"(\"Series\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(10);
        }

        [Test]
        public void where_equal_variable()
        {
            var id = 10;
            _subject = Where(x => x.Id == id);

            _subject.ToString().Should().Be($"(\"Series\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(id);
        }

        [Test]
        public void where_equal_property()
        {
            var movie = new Series { Id = 10 };
            _subject = Where(x => x.Id == movie.Id);

            _subject.Parameters.ParameterNames.Should().HaveCount(1);
            _subject.ToString().Should().Be($"(\"Series\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(movie.Id);
        }

        [Test]
        public void where_equal_joined_property()
        {
            _subject = Where(x => x.QualityProfile.Value.Id == 1);

            _subject.Parameters.ParameterNames.Should().HaveCount(1);
            _subject.ToString().Should().Be($"(\"QualityProfiles\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(1);
        }

        [Test]
        public void where_throws_without_concrete_condition_if_requiresConcreteCondition()
        {
            Expression<Func<Series, Series, bool>> filter = (x, y) => x.Id == y.Id;
            _subject = new WhereBuilder(filter, true, 0);
            Assert.Throws<InvalidOperationException>(() => _subject.ToString());
        }

        [Test]
        public void where_allows_abstract_condition_if_not_requiresConcreteCondition()
        {
            Expression<Func<Series, Series, bool>> filter = (x, y) => x.Id == y.Id;
            _subject = new WhereBuilder(filter, false, 0);
            _subject.ToString().Should().Be($"(\"Series\".\"Id\" = \"Series\".\"Id\")");
        }

        [Test]
        public void where_string_is_null()
        {
            _subject = Where(x => x.CleanTitle == null);

            _subject.ToString().Should().Be($"(\"Series\".\"CleanTitle\" IS NULL)");
        }

        [Test]
        public void where_string_is_null_value()
        {
            string cleanTitle = null;
            _subject = Where(x => x.CleanTitle == cleanTitle);

            _subject.ToString().Should().Be($"(\"Series\".\"CleanTitle\" IS NULL)");
        }

        [Test]
        public void where_equal_null_property()
        {
            var movie = new Series { CleanTitle = null };
            _subject = Where(x => x.CleanTitle == movie.CleanTitle);

            _subject.ToString().Should().Be($"(\"Series\".\"CleanTitle\" IS NULL)");
        }

        [Test]
        public void where_column_contains_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanTitle.Contains(test));

            _subject.ToString().Should().Be($"(\"Series\".\"CleanTitle\" LIKE '%' || @Clause1_P1 || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void where_string_contains_column()
        {
            var test = "small";
            _subject = Where(x => test.Contains(x.CleanTitle));

            _subject.ToString().Should().Be($"(@Clause1_P1 LIKE '%' || \"Series\".\"CleanTitle\" || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void where_column_starts_with_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanTitle.StartsWith(test));

            _subject.ToString().Should().Be($"(\"Series\".\"CleanTitle\" LIKE @Clause1_P1 || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void where_column_ends_with_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanTitle.EndsWith(test));

            _subject.ToString().Should().Be($"(\"Series\".\"CleanTitle\" LIKE '%' || @Clause1_P1)");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void where_in_list()
        {
            var list = new List<int> { 1, 2, 3 };
            _subject = Where(x => list.Contains(x.Id));

            _subject.ToString().Should().Be($"(\"Series\".\"Id\" IN (1, 2, 3))");

            _subject.Parameters.ParameterNames.Should().BeEmpty();
        }

        [Test]
        public void where_in_list_2()
        {
            var list = new List<int> { 1, 2, 3 };
            _subject = Where(x => x.CleanTitle == "test" && list.Contains(x.Id));

            _subject.ToString().Should().Be($"((\"Series\".\"CleanTitle\" = @Clause1_P1) AND (\"Series\".\"Id\" IN (1, 2, 3)))");
        }

        [Test]
        public void where_in_string_list()
        {
            var list = new List<string> { "first", "second", "third" };

            _subject = Where(x => list.Contains(x.CleanTitle));

            _subject.ToString().Should().Be($"(\"Series\".\"CleanTitle\" IN @Clause1_P1)");
        }

        [Test]
        public void enum_as_int()
        {
            _subject = Where(x => x.Status == SeriesStatusType.Upcoming);

            _subject.ToString().Should().Be($"(\"Series\".\"Status\" = @Clause1_P1)");
        }

        [Test]
        public void enum_in_list()
        {
            var allowed = new List<SeriesStatusType> { SeriesStatusType.Upcoming, SeriesStatusType.Continuing };
            _subject = Where(x => allowed.Contains(x.Status));

            _subject.ToString().Should().Be($"(\"Series\".\"Status\" IN @Clause1_P1)");
        }

        [Test]
        public void enum_in_array()
        {
            var allowed = new SeriesStatusType[] { SeriesStatusType.Upcoming, SeriesStatusType.Continuing };
            _subject = Where(x => allowed.Contains(x.Status));

            _subject.ToString().Should().Be($"(\"Series\".\"Status\" IN @Clause1_P1)");
        }
    }
}
