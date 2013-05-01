using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;

namespace NzbDrone.Common.Test.CacheTests
{
    [TestFixture]
    public class CachedFixture
    {
        private Cached<string> _cachedString = new Cached<string>();
        private Worker _worker;

        [SetUp]
        public void SetUp()
        {
            _cachedString = new Cached<string>();
            _worker = new Worker();
        }

        [Test]
        public void should_call_function_once()
        {
            _cachedString.Get("Test", _worker.GetString);
            _cachedString.Get("Test", _worker.GetString);

            _worker.HitCount.Should().Be(1);

        }



        [Test]
        public void multiple_calls_should_return_same_result()
        {
            var first = _cachedString.Get("Test", _worker.GetString);
            var second = _cachedString.Get("Test", _worker.GetString);

            first.Should().Be(second);

        }


        [Test]
        public void should_remove_value_from_set()
        {
            _cachedString.Get("Test", _worker.GetString);

            _cachedString.Remove("Test");

            _cachedString.Get("Test", _worker.GetString);


            _worker.HitCount.Should().Be(2);

        }

        [Test]
        public void remove_none_existing_should_break_things()
        {
            _cachedString.Remove("Test");
        }
    }

    public class Worker
    {
        public int HitCount { get; private set; }

        public string GetString()
        {
            HitCount++;
            return "Hit count is " + HitCount;
        }
    }
}