using System;
using System.Collections.Generic;
using System.Text;
using AutoMoq;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class SearchJobTest
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void start_target_id_less_than_0_throws_exception(int target)
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.Resolve<EpisodeSearchJob>().Start(new ProgressNotification("Test"), target, 0);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void start_secondary_target_id_less_than_0_throws_exception(int target)
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.Resolve<SeasonSearchJob>().Start(new ProgressNotification("Test"), 0, target);
        }
    }
}
