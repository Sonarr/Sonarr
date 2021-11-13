using System.Collections.Generic;
using System.Net.Http;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    public class SeasonSearchFixture : TestBase<TestIndexer>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "<xml></xml>"));
        }

        private void WithIndexer(bool paging, int resultCount)
        {
            var definition = new IndexerDefinition();
            definition.Name = "Test";
            Subject.Definition = definition;

            Subject._supportedPageSize = paging ? 100 : 0;

            var requestGenerator = Mocker.GetMock<IIndexerRequestGenerator>();
            Subject._requestGenerator = requestGenerator.Object;

            var requests = Builder<IndexerRequest>.CreateListOfSize(paging ? 100 : 1)
                .All()
                .WithFactory(() => new IndexerRequest("http://my.feed.local/", HttpAccept.Rss))
                .With(v => v.HttpRequest.Method = HttpMethod.Get)
                .Build();

            var pageable = new IndexerPageableRequestChain();
            pageable.Add(requests);

            requestGenerator.Setup(s => s.GetSearchRequests(It.IsAny<SeasonSearchCriteria>()))
                .Returns(pageable);

            var parser = Mocker.GetMock<IParseIndexerResponse>();
            Subject._parser = parser.Object;

            var results = Builder<ReleaseInfo>.CreateListOfSize(resultCount)
                .Build();

            parser.Setup(s => s.ParseResponse(It.IsAny<IndexerResponse>()))
                   .Returns(results);
        }

        [Test]
        public void should_not_use_offset_if_result_count_is_less_than_90()
        {
            WithIndexer(true, 25);

            Subject.Fetch(new SeasonSearchCriteria { Series = _series, SceneTitles = new List<string> { _series.Title } });

            Mocker.GetMock<IHttpClient>().Verify(v => v.Execute(It.IsAny<HttpRequest>()), Times.Once());
        }

        [Test]
        public void should_not_use_offset_for_sites_that_do_not_support_it()
        {
            WithIndexer(false, 125);

            Subject.Fetch(new SeasonSearchCriteria { Series = _series, SceneTitles = new List<string> { _series.Title } });

            Mocker.GetMock<IHttpClient>().Verify(v => v.Execute(It.IsAny<HttpRequest>()), Times.Once());
        }

        [Test]
        public void should_not_use_offset_if_its_already_tried_10_times()
        {
            WithIndexer(true, 100);

            Subject.Fetch(new SeasonSearchCriteria { Series = _series, SceneTitles = new List<string> { _series.Title } });

            Mocker.GetMock<IHttpClient>().Verify(v => v.Execute(It.IsAny<HttpRequest>()), Times.Exactly(10));
        }
    }
}
