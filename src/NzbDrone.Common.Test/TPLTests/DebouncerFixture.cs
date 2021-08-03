using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.TPL;

namespace NzbDrone.Common.Test.TPLTests
{
    [TestFixture]
    public class DebouncerFixture
    {
        public class Counter
        {
            public int Count { get; private set; }

            public void Hit()
            {
                Count++;
            }
        }

        [Test]
        [Retry(3)]
        public void should_hold_the_call_for_debounce_duration()
        {
            var counter = new Counter();
            var debounceFunction = new Debouncer(counter.Hit, TimeSpan.FromMilliseconds(50));

            debounceFunction.Execute();
            debounceFunction.Execute();
            debounceFunction.Execute();

            counter.Count.Should().Be(0);

            Thread.Sleep(100);

            counter.Count.Should().Be(1);
        }

        [Test]
        [Retry(3)]
        public void should_throttle_calls()
        {
            var counter = new Counter();
            var debounceFunction = new Debouncer(counter.Hit, TimeSpan.FromMilliseconds(50));

            debounceFunction.Execute();
            debounceFunction.Execute();
            debounceFunction.Execute();

            counter.Count.Should().Be(0);

            Thread.Sleep(200);

            debounceFunction.Execute();
            debounceFunction.Execute();
            debounceFunction.Execute();

            Thread.Sleep(200);

            counter.Count.Should().Be(2);
        }

        [Test]
        [Retry(3)]
        public void should_hold_the_call_while_paused()
        {
            var counter = new Counter();
            var debounceFunction = new Debouncer(counter.Hit, TimeSpan.FromMilliseconds(50));

            debounceFunction.Pause();

            debounceFunction.Execute();
            debounceFunction.Execute();

            Thread.Sleep(100);

            counter.Count.Should().Be(0);

            debounceFunction.Execute();
            debounceFunction.Execute();

            Thread.Sleep(100);

            counter.Count.Should().Be(0);

            debounceFunction.Resume();

            Thread.Sleep(20);

            counter.Count.Should().Be(0);

            Thread.Sleep(100);

            counter.Count.Should().Be(1);
        }

        [Test]
        [Retry(3)]
        public void should_handle_pause_reentrancy()
        {
            var counter = new Counter();
            var debounceFunction = new Debouncer(counter.Hit, TimeSpan.FromMilliseconds(50));

            debounceFunction.Pause();
            debounceFunction.Pause();

            debounceFunction.Execute();
            debounceFunction.Execute();

            debounceFunction.Resume();

            Thread.Sleep(100);

            counter.Count.Should().Be(0);

            debounceFunction.Resume();

            Thread.Sleep(100);

            counter.Count.Should().Be(1);
        }
    }
}
