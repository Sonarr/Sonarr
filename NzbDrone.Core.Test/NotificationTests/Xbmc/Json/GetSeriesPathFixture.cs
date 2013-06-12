using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Notifications.Xbmc;
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

        [SetUp]
        public void Setup()
        {
            _settings = new XbmcSettings
            {
                Host = "localhost",
                Port = 8080,
                Username = "xbmc",
                Password = "xbmc",
                AlwaysUpdate = false,
                CleanLibrary = false,
                UpdateLibrary = true
            };

            _response = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":" + 
                        "{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\"" + 
                        ":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\"," +
                        "\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\"" + 
                        ",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":" +
                        "\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\"" +
                        ":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1}," +
                        "{\"file\":\"smb://HOMESERVER/TV/30 Rock/\",\"imdbnumber\":\"79488\",\"label\":\"30 Rock\",\"tvshowid\":2}" +
                        ",{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            Mocker.GetMock<IHttpProvider>()
                  .Setup(
                      s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.IsAny<string>()))
                  .Returns(_response);
        }

        private void WithMatchingTvdbId()
        {
            _series = new Series
                          {
                              TvdbId = 78461,
                              Title = "TV Show"
                          };
        }

        private void WithMatchingTitle()
        {
            _series = new Series
            {
                TvdbId = 1,
                Title = "30 Rock"
            };
        }

        private void WithoutMatchingSeries()
        {
            _series = new Series
            {
                TvdbId = 1,
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

            Subject.GetSeriesPath(_settings, _series).Should().Be("smb://HOMESERVER/TV/8 Simple Rules/");
        }

        [Test]
        public void should_return_path_when_title_matches()
        {
            WithMatchingTitle();

            Subject.GetSeriesPath(_settings, _series).Should().Be("smb://HOMESERVER/TV/30 Rock/");
        }
    }
}
