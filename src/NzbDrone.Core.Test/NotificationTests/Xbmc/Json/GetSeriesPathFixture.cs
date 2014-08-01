using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Json
{
    [TestFixture]
    public class GetSeriesPathFixture : CoreTest<JsonApiProvider>
    {
        private XbmcSettings _settings;
        private Series _series;
        private string _response;
        private List<TvShow> _xbmcSeries;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _xbmcSeries = Builder<TvShow>.CreateListOfSize(3)
                                            .Build()
                                            .ToList();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetSeries(_settings))
                  .Returns(_xbmcSeries);
        }

        private void WithMatchingTvdbId()
        {
            _series = new Series
                          {
                              TvdbId = _xbmcSeries.First().ImdbNumber,
                              Title = "TV Show"
                          };
        }

        private void WithMatchingTitle()
        {
            _series = new Series
            {
                TvdbId = 1000,
                Title = _xbmcSeries.First().Label
            };
        }

        private void WithoutMatchingSeries()
        {
            _series = new Series
            {
                TvdbId = 1000,
                Title = "Does not exist"
            }; 
        }

        [Test]
        public void should_return_null_when_series_is_not_found()
        {
            WithoutMatchingSeries();

            Subject.GetSeriesPath(_settings, _series).Should().BeNull();
        }

        [Test]
        public void should_return_path_when_tvdbId_matches()
        {
            WithMatchingTvdbId();

            Subject.GetSeriesPath(_settings, _series).Should().Be(_xbmcSeries.First().File);
        }

        [Test]
        public void should_return_path_when_title_matches()
        {
            WithMatchingTitle();

            Subject.GetSeriesPath(_settings, _series).Should().Be(_xbmcSeries.First().File);
        }
    }
}
