using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;

namespace NzbDrone.Common.Test.CacheTests
{
    [TestFixture]
    public class CachedDictionaryFixture
    {
        private CachedDictionary<string> _cachedString;
        private DictionaryWorker _worker;

        [SetUp]
        public void SetUp()
        {
            _worker = new DictionaryWorker();
            _cachedString = new CachedDictionary<string>(_worker.GetDict, TimeSpan.FromMilliseconds(100));
        }

        [Test]
        public void should_not_fetch_on_create()
        {
            _worker.HitCount.Should().Be(0);
        }

        [Test]
        public void should_fetch_on_first_call()
        {
            var result = _cachedString.Get("Hi");

            _worker.HitCount.Should().Be(1);

            result.Should().Be("Value");
        }

        [Test]
        public void should_fetch_once()
        {
            var result1 = _cachedString.Get("Hi");
            var result2 = _cachedString.Get("HitCount");

            _worker.HitCount.Should().Be(1);
        }

        [Test]
        public void should_auto_refresh_after_lifetime()
        {
            var result1 = _cachedString.Get("Hi");

            Thread.Sleep(200);

            var result2 = _cachedString.Get("Hi");

            _worker.HitCount.Should().Be(2);
        }

        [Test]
        public void should_refresh_early_if_requested()
        {
            var result1 = _cachedString.Get("Hi");

            Thread.Sleep(10);

            _cachedString.RefreshIfExpired(TimeSpan.FromMilliseconds(1));

            var result2 = _cachedString.Get("Hi");

            _worker.HitCount.Should().Be(2);
        }

        [Test]
        public void should_not_refresh_early_if_not_expired()
        {
            var result1 = _cachedString.Get("Hi");

            _cachedString.RefreshIfExpired(TimeSpan.FromMilliseconds(50));

            var result2 = _cachedString.Get("Hi");

            _worker.HitCount.Should().Be(1);
        }
    }

    public class DictionaryWorker
    {
        public int HitCount { get; private set; }

        public Dictionary<string, string> GetDict()
        {
            HitCount++;

            var result = new Dictionary<string, string>();
            result["Hi"] = "Value";
            result["HitCount"] = "Hit count is " + HitCount;

            return result;
        }
    }
}
