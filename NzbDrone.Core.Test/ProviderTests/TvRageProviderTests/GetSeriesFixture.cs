// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using TvdbLib.Data;
using TvdbLib.Exceptions;

namespace NzbDrone.Core.Test.ProviderTests.TvRageProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class GetSeriesFixture : SqlCeTest
    {
        private const string showinfo = "http://services.tvrage.com/feeds/showinfo.php?key=NW4v0PSmQIoVmpbASLdD&sid=";

        private void WithEmptyResults()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadStream(It.Is<String>(u => u.StartsWith(showinfo)), null))
                    .Returns(new FileStream(@".\Files\TVRage\SeriesInfo_empty.xml", FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        private void WithOneResult()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadStream(It.Is<String>(u => u.StartsWith(showinfo)), null))
                    .Returns(new FileStream(@".\Files\TVRage\SeriesInfo_one.xml", FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        [Test]
        public void should_be_null_when_no_showinfo_is_returned()
        {
            WithEmptyResults();
            Mocker.Resolve<TvRageProvider>().GetSeries(100).Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_series_when_showinfo_is_valid()
        {
            WithOneResult();
            var result = Mocker.Resolve<TvRageProvider>().GetSeries(29999);

            result.ShowId.Should().Be(29999);
            result.Name.Should().Be("Anger Management");
        }
    }
}