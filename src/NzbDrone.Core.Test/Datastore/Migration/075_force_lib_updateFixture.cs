using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class force_lib_updateFixture : MigrationTest<Core.Datastore.Migration.force_lib_update>
    {
        [Test]
        public void should_not_fail_on_empty_db()
        {
            WithTestDb(c => { });

            Mocker.Resolve<ScheduledTaskRepository>().All().Should().BeEmpty();
            Mocker.Resolve<SeriesRepository>().All().Should().BeEmpty();
        }


        [Test]
        public void should_reset_job_last_execution_time()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("ScheduledTasks").Row(new
                {
                    TypeName = "NzbDrone.Core.Tv.Commands.RefreshSeriesCommand",
                    Interval = 10,
                    LastExecution = "2000-01-01 00:00:00"
                });

                c.Insert.IntoTable("ScheduledTasks").Row(new
                {
                    TypeName = "NzbDrone.Core.Backup.BackupCommand",
                    Interval = 10,
                    LastExecution = "2000-01-01 00:00:00"
                });
            });

            var jobs = Mocker.Resolve<ScheduledTaskRepository>().All().ToList();

            jobs.Single(c => c.TypeName == "NzbDrone.Core.Tv.Commands.RefreshSeriesCommand")
                .LastExecution.Year.Should()
                .Be(2014);

            jobs.Single(c => c.TypeName == "NzbDrone.Core.Backup.BackupCommand")
               .LastExecution.Year.Should()
               .Be(2000);
        }


        [Test]
        public void should_reset_series_last_sync_time()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("Series").Row(new
                {
                    Tvdbid = 1,
                    TvRageId =1,
                    Title ="Title1",
                    CleanTitle ="CleanTitle1",
                    Status =1,
                    Images ="",
                    Path ="c:\\test",
                    Monitored =1,
                    SeasonFolder =1,
                    Runtime= 0,
                    SeriesType=0,
                    UseSceneNumbering =0,
                    LastInfoSync = "2000-01-01 00:00:00"
                });

                c.Insert.IntoTable("Series").Row(new
                {
                    Tvdbid = 2,
                    TvRageId = 2,
                    Title = "Title2",
                    CleanTitle = "CleanTitle2",
                    Status = 1,
                    Images = "",
                    Path = "c:\\test2",
                    Monitored = 1,
                    SeasonFolder = 1,
                    Runtime = 0,
                    SeriesType = 0,
                    UseSceneNumbering = 0,
                    LastInfoSync = "2000-01-01 00:00:00"
                });
            });

            var jobs = Mocker.Resolve<SeriesRepository>().All().ToList();

            jobs.Should().OnlyContain(c => c.LastInfoSync.Value.Year == 2014);
        }
    }
}