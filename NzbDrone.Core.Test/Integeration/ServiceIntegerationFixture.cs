using System;
using System.Data;
using System.Linq;
using Autofac;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test.Integeration
{
    [TestFixture(Category = "ServiceIngeneration")]
    public class ServiceIntegerationFixture : CoreTest
    {
        private IContainer _container;

        [SetUp]
        public void Setup()
        {
            WithRealDb();
            var builder = new CentralDispatch().ContainerBuilder;

            builder.Register(c => Db)
                            .As<IDatabase>();

            _container = builder.Build();

            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.ServiceRootUrl)
                    .Returns("http://services.nzbdrone.com");
        }

        [Test]
        public void should_be_able_to_update_scene_mapping()
        {
            _container.Resolve<SceneMappingProvider>().UpdateMappings();
            var mappings = Db.Fetch<SceneMapping>();

            mappings.Should().NotBeEmpty();

            mappings.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.CleanTitle));
            mappings.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.SceneName));
            mappings.Should().OnlyContain(c => c.SeriesId > 0);
        }

        [Test]
        public void should_be_able_to_get_daily_series_ids()
        {
            var dailySeries = _container.Resolve<ReferenceDataProvider>().GetDailySeriesIds();

            dailySeries.Should().NotBeEmpty();
            dailySeries.Should().OnlyContain(c => c > 0);
        }
    }
}