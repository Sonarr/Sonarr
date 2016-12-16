using NUnit.Framework;
using NzbDrone.Test.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Http;
using System.Collections.Specialized;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class HttpHeaderFixture : TestBase
    {
        [TestCase("text/html; charset=\"utf-8\"")]
        [TestCase("text/html; charset=utf-8")]
        public void should_get_encoding(string contentType)
        {
            var headers = new NameValueCollection();

            headers.Add("Content-Type", contentType);

            var httpheader = new HttpHeader(headers);

            Assert.DoesNotThrow(() => httpheader.GetEncodingFromContentType());
        }
    }
}
