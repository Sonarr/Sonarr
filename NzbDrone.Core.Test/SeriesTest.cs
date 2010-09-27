using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Controllers;

// ReSharper disable InconsistentNaming
namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class SeriesTest
    {
        [Test]
        [Description("This test will confirm that a folder will be skipped if it has been resolved to a series already assigned to another folder")]
        public void skip_same_series_diffrent_folder()
        {
            //Arrange
            var seriesProvider = new SeriesController(new Mock<Ilog>(), new Mock<IDiskController>(), new Mock<IConfigController>(), new )
        }
    }
}
