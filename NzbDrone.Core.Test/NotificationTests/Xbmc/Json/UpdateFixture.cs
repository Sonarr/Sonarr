using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
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
    public class UpdateFixture : CoreTest<JsonApiProvider>
    {
        private XbmcSettings _settings;
        const string _expectedJson = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"properties\":[\"file\",\"imdbnumber\"]},\"id\":10}";

        private const string _tvshowsResponse = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":" +
                                        "{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\"" +
                                        ":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\"," +
                                        "\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\"" +
                                        ",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":" +
                                        "\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\"" +
                                        ":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1}," +
                                        "{\"file\":\"smb://HOMESERVER/TV/30 Rock/\",\"imdbnumber\":\"79488\",\"label\":\"30 Rock\",\"tvshowid\":2}" +
                                        ",{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

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

            Mocker.GetMock<IHttpProvider>()
                .Setup(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password,
                        It.Is<string>(e => e.Replace(" ", "").Replace("\r\n", "").Replace("\t", "") == _expectedJson.Replace(" ", ""))))
                .Returns(_tvshowsResponse);
        }

        [Test]
        public void should_update_using_series_path()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.TvdbId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.Is<String>(
                    e => e.Replace(" ", "")
                      .Replace("\r\n", "")
                      .Replace("\t", "")
                      .Contains("\"params\":{\"directory\":\"smb://HOMESERVER/TV/30Rock/\"}"))))
                  .Returns("{\"id\":55,\"jsonrpc\":\"2.0\",\"result\":\"OK\"}");

            Subject.Update(_settings, fakeSeries);

            Mocker.GetMock<IHttpProvider>()
                  .Verify(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.Is<String>(
                    e => e.Replace(" ", "")
                      .Replace("\r\n", "")
                      .Replace("\t", "")
                      .Contains("\"params\":{\"directory\":\"smb://HOMESERVER/TV/30Rock/\"}"))), Times.Once());
        }

        [Test]
        public void should_update_all_paths_when_series_path_not_found()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.TvdbId = 1)
                .With(s => s.Title = "Not 30 Rock")
                .Build();

             Mocker.GetMock<IHttpProvider>()
                   .Setup(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.Is<String>(
                     e => !e.Replace(" ", "")
                      .Replace("\r\n", "")
                      .Replace("\t", "")
                      .Contains("\"params\":{\"directory\":\"smb://HOMESERVER/TV/30Rock/\"}"))))
                   .Returns("{\"id\":55,\"jsonrpc\":\"2.0\",\"result\":\"OK\"}");

             Subject.Update(_settings, fakeSeries);

             Mocker.GetMock<IHttpProvider>()
                   .Verify(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.Is<String>(
                     e => e.Replace(" ", "")
                      .Replace("\r\n", "")
                      .Replace("\t", "")
                      .Contains("\"params\":{\"directory\":\"smb://HOMESERVER/TV/30Rock/\"}"))), Times.Never());
        }
    }
}
