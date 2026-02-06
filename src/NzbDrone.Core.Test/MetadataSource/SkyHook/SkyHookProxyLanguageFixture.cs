using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    public class SkyHookProxyLanguageFixture : CoreTest<SkyHookProxy>
    {
        [Test]
        public void GetSeriesInfo_should_use_TvdbMetadataLanguage_from_config()
        {
            HttpRequest capturedRequest = null;

            var minimalShow = new ShowResource
            {
                TvdbId = 121361,
                Title = "Test",
                Slug = "test",
                Status = "ended",
                Network = "Test",
                TimeOfDay = new TimeOfDayResource { Hours = 20, Minutes = 0 }
            };

            var json = minimalShow.ToJson();

            Mocker.GetMock<IConfigService>()
                .Setup(c => c.TvdbMetadataLanguage)
                .Returns("de");

            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Get<ShowResource>(It.IsAny<HttpRequest>()))
                .Callback<HttpRequest>(r => capturedRequest = r)
                .Returns<HttpRequest>(req =>
                {
                    var response = new HttpResponse(req, new HttpHeader(), json, HttpStatusCode.OK);
                    return new HttpResponse<ShowResource>(response);
                });

            var result = Subject.GetSeriesInfo(121361);

            capturedRequest.Should().NotBeNull();
            capturedRequest.Url.FullUri.Should().Contain("/de/");
            result.Item1.Title.Should().Be("Test");
        }

        [Test]
        public void SearchForNewSeries_should_use_TvdbMetadataLanguage_from_config()
        {
            HttpRequest capturedRequest = null;

            var minimalShow = new ShowResource
            {
                TvdbId = 78804,
                Title = "Doctor Who",
                Slug = "doctor-who",
                Status = "ended",
                Network = "BBC",
                TimeOfDay = new TimeOfDayResource { Hours = 19, Minutes = 0 }
            };

            var json = "[" + minimalShow.ToJson() + "]";

            Mocker.GetMock<IConfigService>()
                .Setup(c => c.TvdbMetadataLanguage)
                .Returns("fr");

            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Get<List<ShowResource>>(It.IsAny<HttpRequest>()))
                .Callback<HttpRequest>(r => capturedRequest = r)
                .Returns<HttpRequest>(req =>
                {
                    var response = new HttpResponse(req, new HttpHeader(), json, HttpStatusCode.OK);
                    return new HttpResponse<List<ShowResource>>(response);
                });

            var result = Subject.SearchForNewSeries("Doctor Who");

            capturedRequest.Should().NotBeNull();
            capturedRequest.Url.FullUri.Should().Contain("/fr/");
            result.Should().ContainSingle();
            result[0].Title.Should().Be("Doctor Who");
        }
    }
}
