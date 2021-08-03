using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Json
{
    [TestFixture]
    public class UpdateFixture : CoreTest<XbmcService>
    {
        private const int TVDB_ID = 5;
        private XbmcSettings _settings;
        private List<TvShow> _xbmcSeries;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _xbmcSeries = Builder<TvShow>.CreateListOfSize(3)
                                         .TheFirst(1)
                                         .With(s => s.ImdbNumber = TVDB_ID.ToString())
                                         .Build()
                                         .ToList();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetSeries(_settings))
                  .Returns(_xbmcSeries);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetActivePlayers(_settings))
                  .Returns(new List<ActivePlayer>());
        }

        [Test]
        public void should_update_using_series_path()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(s => s.TvdbId = TVDB_ID)
                                        .Build();

            Subject.Update(_settings, series);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Verify(v => v.UpdateLibrary(_settings, It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_update_all_paths_when_series_path_not_found()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                                            .With(s => s.TvdbId = 1000)
                                            .With(s => s.Title = "Not 30 Rock")
                                            .Build();

            Subject.Update(_settings, fakeSeries);

            Mocker.GetMock<IXbmcJsonApiProxy>()
                   .Verify(v => v.UpdateLibrary(_settings, null), Times.Once());
        }
    }
}
