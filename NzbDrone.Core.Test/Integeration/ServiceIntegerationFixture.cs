using System;
using System.Data;
using System.Linq;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test.Integeration
{
    [TestFixture(Category = "ServiceIngeneration")]
    [Explicit]
    public class ServiceIntegerationFixture : CoreTest
    {
        private KernelBase _kernel;

        [SetUp]
        public void Setup()
        {
            WithRealDb();
            _kernel = new StandardKernel();
            _kernel.Bind<IDatabase>().ToConstant(Db);

            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.ServiceRootUrl)
                    .Returns("http://services.nzbdrone.com");

        }

        [Test]
        public void should_be_able_to_update_scene_mapping()
        {
            _kernel.Get<SceneMappingProvider>().UpdateMappings();
            var mappings = Db.Fetch<SceneMapping>();

            mappings.Should().NotBeEmpty();

            mappings.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.CleanTitle));
            mappings.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.SceneName));
            mappings.Should().OnlyContain(c => c.SeriesId > 0);
        }

        [Test]
        public void should_be_able_to_get_daily_series_ids()
        {
            var dailySeries = _kernel.Get<ReferenceDataProvider>().GetDailySeriesIds();

            dailySeries.Should().NotBeEmpty();
            dailySeries.Should().OnlyContain(c => c > 0);
        }


        [Test]
        public void should_be_able_to_submit_exceptions()
        {
            ReportingService.SetupExceptronDriver();

            try
            {
                ThrowException();
            }
            catch (Exception e)
            {
                var log = new LogEventInfo
                              {
                                  LoggerName = "LoggerName.LoggerName.LoggerName.LoggerName",
                                  Exception = e,
                                  Message = "New message string. New message string.",
                              };

                var hash = ReportingService.ReportException(log);

                hash.Should().HaveLength(8);
            }


        }



    }
}