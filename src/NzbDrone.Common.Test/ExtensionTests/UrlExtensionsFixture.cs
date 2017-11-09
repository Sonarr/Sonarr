using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests
{
    [TestFixture]
    public class UrlExtensionsFixture
    {
        [TestCase("http://my.local/url")]
        [TestCase("https://my.local/url")]
        public void should_report_as_valid_url(string url)
        {
            url.IsValidUrl().Should().BeTrue();
        }

        [TestCase("")]
        [TestCase(" http://my.local/url")]
        [TestCase("http://my.local/url ")]
        public void should_report_as_invalid_url(string url)
        {
            url.IsValidUrl().Should().BeFalse();
        }
    }
}
