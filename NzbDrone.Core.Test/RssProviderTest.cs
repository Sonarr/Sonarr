using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using AutoMoq;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Feed;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class RssProviderTest
    // ReSharper disable InconsistentNaming
    {
        [Test]

        public void Download_feed_test()
        {
            var mocker = new AutoMoqer();

            var xmlReader = XmlReader.Create(File.OpenRead(".\\Files\\Rss\\nzbsorg.xml"));

            mocker.GetMock<HttpProvider>()
                .Setup(h => h.DownloadXml(It.IsAny<String>()))
                .Returns(xmlReader);

            mocker.Resolve<MockFeedProvider>().Fetch();
        }

    }

    public class MockFeedProvider : FeedProviderBase
    {
        public MockFeedProvider(SeriesProvider seriesProvider, ISeasonProvider seasonProvider, IEpisodeProvider episodeProvider, IConfigProvider configProvider, HttpProvider httpProvider)
            : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider)
        {
        }

        protected override string[] URL
        {
            get { return new[] { "www.google.com" }; }
        }

        protected override string Name
        {
            get { return "MyName"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }
    }
}
