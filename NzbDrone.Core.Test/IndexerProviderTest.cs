using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class IndexerProviderTest
    {
        [Test]
        public void AllIndexers()
        {
            //
            // TODO: Add test logic here
            //

            //Setup
            var list = new List<Indexer>();
            list.Add(new Indexer { IndexerName = "Test1", RssUrl = "http://www.test1.com/rss.php", Enabled = true, Order = 1 });
            list.Add(new Indexer { IndexerName = "Test2", RssUrl = "http://www.test2.com/rss.php", Enabled = false, Order = 4 });
            list.Add(new Indexer { IndexerName = "Test3", RssUrl = "http://www.test3.com/rss.php", Enabled = true, Order = 3 });
            list.Add(new Indexer { IndexerName = "Test4", RssUrl = "http://www.test4.com/rss.php", Enabled = false, Order = 2 });

            var repo = new Mock<IRepository>();
            repo.Setup(r => r.All<Indexer>()).Returns(list.AsQueryable());

            var target = new IndexerProvider(repo.Object);

            //Act
            var result = target.AllIndexers();

            //Assert

            Assert.AreEqual(result.Last().IndexerName, "Test2");
        }

        [Test]
        public void EnabledIndexers()
        {
            //
            // TODO: Add test logic here
            //

            //Setup
            var list = new List<Indexer>();
            list.Add(new Indexer { IndexerName = "Test1", RssUrl = "http://www.test1.com/rss.php", Enabled = true, Order = 1 });
            list.Add(new Indexer { IndexerName = "Test2", RssUrl = "http://www.test2.com/rss.php", Enabled = false, Order = 4 });
            list.Add(new Indexer { IndexerName = "Test3", RssUrl = "http://www.test3.com/rss.php", Enabled = true, Order = 3 });
            list.Add(new Indexer { IndexerName = "Test4", RssUrl = "http://www.test4.com/rss.php", Enabled = false, Order = 2 });

            var repo = new Mock<IRepository>();
            repo.Setup(r => r.All<Indexer>()).Returns(list.AsQueryable());

            var target = new IndexerProvider(repo.Object);

            //Act
            var result = target.EnabledIndexers();

            //Assert
            Assert.AreEqual(result.First().IndexerName, "Test1");
            Assert.AreEqual(result.Last().IndexerName, "Test3");
        }
    }
}
