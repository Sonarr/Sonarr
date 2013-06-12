

using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Xbmc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class XbmcProviderTest : CoreTest
    {
        private XbmcSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new XbmcSettings
                            {
                                Host = "localhost",
                                Port = 8080,
                                Username = "xbmc",
                                Password = "xbmc",
                                AlwaysUpdate = false,
                                CleanLibrary = false,
                                UpdateLibrary = true
                            };
        }
    }
}