using System;

using Moq;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class SearchJobTest:CoreTest
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void start_target_id_less_than_0_throws_exception(int target)
        {
            WithStrictMocker();
            //Mocker.Resolve<EpisodeSearchJob>().Start(new ProgressNotification("Test"), target, 0);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void start_secondary_target_id_less_than_0_throws_exception(int target)
        {
            WithStrictMocker();
            //Mocker.Resolve<SeasonSearchJob>().Start(new ProgressNotification("Test"), 0, target);
        }
    }
}
