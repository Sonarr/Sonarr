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
    public class EpisodeSearchJobTest:SqlCeTest
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        [ExpectedException(typeof(ArgumentException))]
        public void start_target_id_less_than_0_throws_exception(int target)
        {
            WithStrictMocker();
            Mocker.Resolve<EpisodeSearchJob>().Start(new ProgressNotification("Test"), new { EpisodeId = target });
        }

        [TestCase(-1)]
        [TestCase(-100)]
        [ExpectedException(typeof(ArgumentException))]
        public void start_secondary_target_id_less_than_0_throws_exception(int target)
        {
            WithStrictMocker();
            Mocker.Resolve<SeasonSearchJob>().Start(new ProgressNotification("Test"), new { SeriesId = 1, SeasonNumber = target });
        }
    }
}
