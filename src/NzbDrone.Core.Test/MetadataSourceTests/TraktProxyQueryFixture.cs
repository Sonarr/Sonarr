//using System;
//using System.Collections.Generic;
//using Moq;
//using NUnit.Framework;
//using NzbDrone.Common.Http;
//using NzbDrone.Core.MetadataSource;
//using NzbDrone.Core.MetadataSource.Trakt;
//using NzbDrone.Core.Test.Framework;
//using NzbDrone.Test.Common;
//
//namespace NzbDrone.Core.Test.MetadataSourceTests
//{
//    [TestFixture]
//    public class TraktProxyQueryFixture : CoreTest<TraktProxy>
//    {
//        [TestCase("tvdb:78804", "/78804/")]
//        [TestCase("TVDB:78804", "/78804/")]
//        [TestCase("TVDB: 78804 ", "/78804/")]
//        public void search_by_lookup(string title, string expectedPartialQuery)
//        {
//            Assert.Throws<TraktException>(() => Subject.SearchForNewSeries(title));
//
//            Mocker.GetMock<IHttpClient>()
//                .Verify(v => v.Get<Show>(It.Is<HttpRequest>(d => d.Url.ToString().Contains(expectedPartialQuery))), Times.Once());
//
//            ExceptionVerification.ExpectedWarns(1);
//        }
//
//        [TestCase("imdb:tt0436992", "tt0436992")]
//        [TestCase("imdb:0436992", "tt0436992")]
//        [TestCase("IMDB:0436992", "tt0436992")]
//        [TestCase("IMDB: 0436992 ", "tt0436992")]
////        [TestCase("The BigBangTheory", "the+bigbangtheory")]
////        [TestCase("TheBigBangTheory", "the+big+bang+theory")]
////        [TestCase(" TheBigBangTheory", "the+big+bang+theory")]
//        [TestCase("Agents of S.H.I.E.L.D.", "agents+of+s.h.i.e.l.d.")]
//        [TestCase("Marvel's Agents of S.H.I.E.L.D.", "marvels+agents+of+s.h.i.e.l.d.")]
////        [TestCase("Marvel'sAgentsOfS.H.I.E.L.D.", "marvels+agents+of+s.h.i.e.l.d.")]
//        [TestCase("Utopia (US) (2014)", "utopia+us+2014")]
//        [TestCase("Utopia US 2014", "utopia+us+2014")]
////        [TestCase("UtopiaUS2014", "utopia+us+2014")]
//        [TestCase("@Midnight", "midnight")]
////        [TestCase("The4400", "the+4400")]
////        [TestCase("StargateSG-1", "stargate+sg-1")]
////        [TestCase("Warehouse13", "warehouse+13")]
////        [TestCase("Ben10AlienForce", "ben+10+alien+force")]
////        [TestCase("FridayThe13thTheSeries","friday+the+13th+the+series")]
//        [TestCase("W1A", "w1a")]
//        [TestCase("O2Be", "o2be")]
////        [TestCase("TeenMom2", "teen+mom+2")]
//        [TestCase("123-456-789", "123-456-789")]
////        [TestCase("BuckRodgersInThe25thCentury", "buck+rodgers+in+the+25th+century")]
////        [TestCase("EPDaily", "ep+daily")]
//        [TestCase("6ad072c8-d000-4ed5-97d5-324858c45774", "6ad072c8-d000-4ed5-97d5-324858c45774")]
//        [TestCase("6AD072C8-D000-4ED5-97D5-324858C45774", "6ad072c8-d000-4ed5-97d5-324858c45774")]
//        [TestCase("MythBusters", "mythbusters")]
//        public void search_by_query(string title, string expectedPartialQuery)
//        {
//            expectedPartialQuery = String.Format("query={0}&", expectedPartialQuery);
//
//            Assert.Throws<TraktException>(() => Subject.SearchForNewSeries(title));
//
//            Mocker.GetMock<IHttpClient>()
//                .Verify(v => v.Get<List<Show>>(It.Is<HttpRequest>(d => d.Url.ToString().Contains(expectedPartialQuery))), Times.Once());
//
//            ExceptionVerification.ExpectedWarns(1);
//        }
//    }
//}
