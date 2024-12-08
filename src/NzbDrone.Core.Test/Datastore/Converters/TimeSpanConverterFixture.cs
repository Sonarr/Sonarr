using System;
using System.Data.SQLite;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters;

[TestFixture]
public class TimeSpanConverterFixture : CoreTest<TimeSpanConverter>
{
    private SQLiteParameter _param;

    [SetUp]
    public void Setup()
    {
        _param = new SQLiteParameter();
    }

    [Test]
    public void should_return_string_when_saving_timespan_to_db()
    {
        var span = TimeSpan.FromMilliseconds(10);

        Subject.SetValue(_param, span);
        _param.Value.Should().Be(span.ToString());
    }

    [Test]
    public void should_return_timespan_when_getting_string_from_db()
    {
        var span = TimeSpan.FromMilliseconds(10);

        Subject.Parse(span.ToString()).Should().Be(span);
    }

    [Test]
    public void should_return_zero_timespan_for_db_null_value_when_getting_from_db()
    {
        Subject.Parse(null).Should().Be(TimeSpan.Zero);
    }
}
