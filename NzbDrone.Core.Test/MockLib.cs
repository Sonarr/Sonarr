using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NzbDrone.Core.Controllers;

namespace NzbDrone.Core.Test
{
    /// <summary>
    /// Provides the standard Mocks needed for a typical test
    /// </summary>
    static class MockLib
    {
        public static string[] StandardSeries
        {
            get { return new string[] { "C:\\TV\\The Simpsons", "C:\\TV\\Family Guy" }; }
        }


        public static IConfigController StandardConfig
        {
            get
            {
                var mock = new Mock<IConfigController>();
                mock.SetupGet(c => c.SeriesRoot).Returns("C:\\");
                return mock.Object;
            }
        }

        public static IDiskController StandardDisk
        {
            get
            {
                var mock = new Mock<IDiskController>();
                mock.Setup(c => c.GetDirectories(It.IsAny<String>())).Returns(StandardSeries);
                mock.Setup(c => c.Exists(It.Is<String>(d => StandardSeries.Contains(d)))).Returns(true);
                return mock.Object;
            }
        }
    }
}
