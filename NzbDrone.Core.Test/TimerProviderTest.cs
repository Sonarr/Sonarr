using System;
using System.Linq;
using System.Collections.Generic;
using AutoMoq;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Timers;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class TimerProviderTest
    {
        [Test]
        public void Run_Timers()
        {

            IEnumerable<ITimer> fakeTimers = new List<ITimer> { new FakeTimer() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.SetConstant(fakeTimers);

            var timerProvider = mocker.Resolve<TimerProvider>();
            timerProvider.Initialize();
            timerProvider.Run();

        }

        [Test]
        public void Init_Timers()
        {
            var fakeTimer = new FakeTimer();
            IEnumerable<ITimer> fakeTimers = new List<ITimer> { fakeTimer };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.SetConstant(fakeTimers);

            var timerProvider = mocker.Resolve<TimerProvider>();
            timerProvider.Initialize();

            var timers = timerProvider.All();


            //Assert
            Assert.Count(1, timers);
            Assert.AreEqual(fakeTimer.DefaultInterval, timers[0].Interval);
            Assert.AreEqual(fakeTimer.Name, timers[0].Name);
            Assert.AreEqual(fakeTimer.GetType().ToString(), timers[0].TypeName);
            Assert.AreEqual(DateTime.MinValue, timers[0].LastExecution);
            Assert.IsTrue(timers[0].Enable);


        }

        [Test]
        public void Init_Timers_only_registers_once()
        {
            var repo = MockLib.GetEmptyRepository();

            for (int i = 0; i < 2; i++)
            {
                var fakeTimer = new FakeTimer();
                IEnumerable<ITimer> fakeTimers = new List<ITimer> { fakeTimer };
                var mocker = new AutoMoqer();

                mocker.SetConstant(repo);
                mocker.SetConstant(fakeTimers);

                var timerProvider = mocker.Resolve<TimerProvider>();
                timerProvider.Initialize();
            }

            var mocker2 = new AutoMoqer();

            mocker2.SetConstant(repo);
            var assertTimerProvider = mocker2.Resolve<TimerProvider>();

            var timers = assertTimerProvider.All();


            //Assert
            Assert.Count(1, timers);
        }



    }

    public class FakeTimer : ITimer
    {
        public string Name
        {
            get { return "FakeTimer"; }
        }

        public int DefaultInterval
        {
            get { return 15; }
        }

        public void Start()
        {
        }
    }
}