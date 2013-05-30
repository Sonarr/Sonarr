using System.Collections.Generic;
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

        [Test]
        public void get_without_callback_should_throw_on_invalid_key()
        {
            Assert.Throws<KeyNotFoundException>(() => _cachedString.Get("InvalidKey"));
        }

        [Test]
        public void should_be_able_to_update_key()
        {
            _cachedString.Set("Key", "Old");
            _cachedString.Set("Key", "New");

            _cachedString.Get("Key").Should().Be("New");
        }

        [Test]
        public void should_store_null()
        {
            int hitCount = 0;


            for (int i = 0; i < 10; i++)
            {
                _cachedString.Get("key", () =>
                {
                    hitCount++;
                    return null;
                }); 
            }

            hitCount.Should().Be(1);
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