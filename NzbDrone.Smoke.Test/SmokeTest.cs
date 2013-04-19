using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Api.Series;
using RestSharp;

namespace NzbDrone.Smoke.Test
{
    [TestFixture]
    public class SmokeTest : SmokeTestBase
    {
        [Test]
        public void start_application()
        {
            Get<List<SeriesResource>>(new RestRequest("series"));
        }

        [Test]
        [Ignore]
        public void add_series_without_required_fields_should_return_400()
        {
            var addSeriesRequest = new RestRequest("series") { RequestFormat = DataFormat.Json };
            addSeriesRequest.AddBody(new SeriesResource());
            Post<SeriesResource>(addSeriesRequest);
        }

    }
}