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
        public void should_throttle_cals()
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


    }
}