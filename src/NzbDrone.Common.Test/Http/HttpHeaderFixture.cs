using NUnit.Framework;
using FluentAssertions;
using NzbDrone.Test.Common;
using System;
using System.Text;
using NzbDrone.Common.Http;
using System.Collections.Specialized;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class HttpHeaderFixture : TestBase
    {
        [TestCase("text/html; charset=\"utf-8\"", "utf-8")]
        [TestCase("text/html; charset=utf-8", "utf-8")]
        public void should_get_encoding_from_content_type_header(string contentType, string charsetExpected)
        {
            var headers = new NameValueCollection();

            headers.Add("Content-Type", contentType);

            var httpheader = new HttpHeader(headers);

            httpheader.GetEncodingFromContentType().Should().Be(Encoding.GetEncoding(charsetExpected));
        }

        [TestCase("text/html; charset=asdasd")]
        public void should_throw_when_invalid_encoding_is_in_content_type_header(string contentType)
        {
            var headers = new NameValueCollection();

            headers.Add("Content-Type", contentType);

            var httpheader = new HttpHeader(headers);

            Action action = () => httpheader.GetEncodingFromContentType();
            action.ShouldThrow<ArgumentException>();
        }
    }
}
