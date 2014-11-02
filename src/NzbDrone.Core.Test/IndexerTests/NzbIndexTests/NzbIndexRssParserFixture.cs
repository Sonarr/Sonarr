using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.NzbIndex;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NzbIndexTests
{
    public class NzbIndexRssParserFixture : CoreTest<NzbIndexRssParser>
    {

        [TestCase("<rss version=\"2.0\"><description><![CDATA[<p><font color=\"gray\">alt.binaries.multimedia, alt.binaries.teevee</font><br /><b>2.59 GB</b><br />1 hour<br /><font color=\"#3DA233\">64 files (3564 parts)</font><font color=\"gray\">by teevee@4u.tv (teevee)</font><br /><font color=\"#E2A910\">1 NFO | 10 PAR2 | 48 ARCHIVE</font> - <a href=\"http://www.nzbindex.nl/nfo/69196805/93922-FULL-a.b.teeveeEFNet-Titanic.2012.S01E03.720p.BluRay.x264-GaGE-1782-titanic.s01e03.720p.gage.sample.mkv.nzb/?q=\" target=\"_blank\">View NFO</a></p>]]></description></rss>")]
        [TestCase("<rss version=\"2.0\"><description><![CDATA[<p><font color=\"#E2A910\">1 NFO | 10 PAR2 | 48 ARCHIVE</font> - <a href=\"http://www.nzbindex.nl/nfo/69196805/93922-FULL-a.b.teeveeEFNet-Titanic.2012.S01E03.720p.BluRay.x264-GaGE-1782-titanic.s01e03.720p.gage.sample.mkv.nzb/?q=\" target=\"_blank\">View NFO</a></p>]]></description></rss>")]
        public void Parse_Nfo_link(string xml)
        {
            var result = Subject.GetNfoUrl(XElement.Parse(xml));
            result.Should().NotBeNullOrWhiteSpace();
        }

        [TestCase("http://www.nzbindex.nl/download/69196805/93922-FULL-a.b.teeveeEFNet-Titanic.2012.S01E03.720p.BluRay.x264-GaGE-1782-titanic.s01e03.720p.gage.sample.mkv.nzb")]
        [TestCase("http://www.nzbindex.nl/download/69196969/93923-FULL-a.b.teeveeEFNet-Britains.Got.More.Talent.S06E04.PDTV.x264-C4TV-0136-britains.got.more.talent.s06e04.pdtv.x264-c4tv.nfo.nzb")]
        public void Parse_File_from_Url(string url)
        {
            var result = Subject.GetFileFromUrl(url);
            result.Should().NotBeNullOrWhiteSpace();
        }
    }
}
