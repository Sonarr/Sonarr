using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class JsonApiProviderFixture : CoreTest<JsonApiProvider>
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
