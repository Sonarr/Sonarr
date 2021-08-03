using System;
using System.Threading;
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
        public void should_be_able_to_update_key()
        {
            _cachedString.Set("Key", "Old");
            _cachedString.Set("Key", "New");

            _cachedString.Find("Key").Should().Be("New");
        }

        [Test]
        public void should_be_able_to_remove_key()
        {
            _cachedString.Set("Key", "Value");

            _cachedString.Remove("Key");

            _cachedString.Find("Key").Should().BeNull();
        }

        [Test]
        public void should_be_able_to_remove_non_existing_key()
        {
            _cachedString.Remove("Key");
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

        [Test]
        [Platform(Exclude = "MacOsX")]
        public void should_honor_ttl()
        {
            int hitCount = 0;
            _cachedString = new Cached<string>();

            for (int i = 0; i < 10; i++)
            {
                _cachedString.Get("key",
                    () =>
                    {
                        hitCount++;
                        return null;
                    },
                    TimeSpan.FromMilliseconds(300));

                Thread.Sleep(100);
            }

            hitCount.Should().BeInRange(3, 6);
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
