using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Indexers.Omgwtfnzbs;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    public class SeasonSearchFixture : TestBase<FetchFeedService>
    {
        private Series _series;
        private IIndexer _newznab;
        private IIndexer _omgwtfnzbs;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();

            _newznab = new Newznab();

            _newznab.Definition = new IndexerDefinition();
            _newznab.Definition.Name = "nzbs.org";
            _newznab.Definition.Settings = new NewznabSettings
            {
                ApiKey = "",
                Url = "http://nzbs.org"
            };

            _omgwtfnzbs = new Omgwtfnzbs();

            _omgwtfnzbs.Definition = new IndexerDefinition();
            _omgwtfnzbs.Definition.Name = "omgwtfnzbs";
            _omgwtfnzbs.Definition.Settings = new OmgwtfnzbsSettings
                                              {
                                                  ApiKey = "",
                                                  Username = "NzbDrone"
                                              };
        }

        private void WithResults(int count)
        {
            var results = Builder<ReleaseInfo>.CreateListOfSize(count)
                .Build();

            Mocker.GetMock<IIndexerParsingService>()
                  .Setup(s => s.Parse(It.IsAny<IIndexer>(), It.IsAny<String>(), It.IsAny<String>()))
                  .Returns(results);

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>())).Returns("<xml></xml>");
        }

        [Test]
        public void should_not_use_offset_if_result_count_is_less_than_90()
        {
            WithResults(25);
            Subject.Fetch(_newznab, new SeasonSearchCriteria { Series = _series, SceneTitle =  _series.Title });

            Mocker.GetMock<IHttpProvider>().Verify(v => v.DownloadString(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_not_use_offset_for_sites_that_do_not_support_it()
        {
            WithResults(25);
            Subject.Fetch(_omgwtfnzbs, new SeasonSearchCriteria { Series = _series, SceneTitle = _series.Title });

            Mocker.GetMock<IHttpProvider>().Verify(v => v.DownloadString(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_not_use_offset_if_its_already_tried_10_times()
        {
            WithResults(100);
            Subject.Fetch(_newznab, new SeasonSearchCriteria { Series = _series, SceneTitle = _series.Title });

            Mocker.GetMock<IHttpProvider>().Verify(v => v.DownloadString(It.IsAny<String>()), Times.Exactly(11));
        }
    }
}
