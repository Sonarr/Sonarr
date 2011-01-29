using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using Rss;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class RssProviderTest
    {
        [Test]
        public void GetFeed()
        {
            //Setup
            var feedInfo = new FeedInfoModel("NzbMatrix", @"Files\Feed.nzbmatrix.com.xml");
            var target = new RssProvider();

            //Act
            var enumerable = target.GetFeed(feedInfo);
            var result = new List<RssItem>();
            result.AddRange(enumerable);

            //Assert
            Assert.GreaterThan(result.Count, 1); //Assert that the number of Items in the feed is greater than 1
        }
    }
}
