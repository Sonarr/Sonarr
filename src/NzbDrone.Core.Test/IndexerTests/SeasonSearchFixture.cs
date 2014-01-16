using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    public class SeasonSearchFixture : TestBase<FetchFeedService>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>())).Returns("<xml></xml>");
        }

        private IndexerBase<TestIndexerSettings> WithIndexer(bool paging, int resultCount)
        {
            var results = Builder<ReleaseInfo>.CreateListOfSize(resultCount)
                .Build();

            var indexer = Mocker.GetMock<IndexerBase<TestIndexerSettings>>();

            indexer.Setup(s => s.Parser.Process(It.IsAny<String>(), It.IsAny<String>()))
                   .Returns(results);

            indexer.Setup(s => s.GetSeasonSearchUrls(It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Int32>()))
                .Returns(new List<string> { "http://www.nzbdrone.com" });

            indexer.SetupGet(s => s.SupportsPaging).Returns(paging);

            var definition = new IndexerDefinition();
            definition.Name = "Test";

            indexer.SetupGet(s => s.Definition)
                .Returns(definition);

            return indexer.Object;
        }

        [Test]
        public void should_not_use_offset_if_result_count_is_less_than_90()
        {
            var indexer = WithIndexer(true, 25);
            Subject.Fetch(indexer, new SeasonSearchCriteria { Series = _series, SceneTitle =  _series.Title });

            Mocker.GetMock<IHttpProvider>().Verify(v => v.DownloadString(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_not_use_offset_for_sites_that_do_not_support_it()
        {
            var indexer = WithIndexer(false, 125);
            Subject.Fetch(indexer, new SeasonSearchCriteria { Series = _series, SceneTitle = _series.Title });

            Mocker.GetMock<IHttpProvider>().Verify(v => v.DownloadString(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_not_use_offset_if_its_already_tried_10_times()
        {
            var indexer = WithIndexer(true, 100);
            Subject.Fetch(indexer, new SeasonSearchCriteria { Series = _series, SceneTitle = _series.Title });

            Mocker.GetMock<IHttpProvider>().Verify(v => v.DownloadString(It.IsAny<String>()), Times.Exactly(10));
        }
    }

    public class TestIndexerSettings : IProviderConfig
    {
        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
