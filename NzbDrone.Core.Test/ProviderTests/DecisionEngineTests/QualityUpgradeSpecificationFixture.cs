// ReSharper disable RedundantUsingDirective

using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QualityUpgradeSpecificationFixture : SqlCeTest
    {
        public static object[] IsUpgradeTestCases =
        {
            new object[] { QualityTypes.SDTV, false, QualityTypes.SDTV, true, QualityTypes.SDTV, true },
            new object[] { QualityTypes.WEBDL720p, false, QualityTypes.WEBDL720p, true, QualityTypes.WEBDL720p, true },
            new object[] { QualityTypes.SDTV, false, QualityTypes.SDTV, false, QualityTypes.SDTV, false },
            new object[] { QualityTypes.SDTV, false, QualityTypes.DVD, true, QualityTypes.SDTV, false },
            new object[] { QualityTypes.WEBDL720p, false, QualityTypes.HDTV720p, true, QualityTypes.Bluray720p, false },
            new object[] { QualityTypes.WEBDL720p, false, QualityTypes.HDTV720p, true, QualityTypes.WEBDL720p, false },
            new object[] { QualityTypes.WEBDL720p, false, QualityTypes.WEBDL720p, false, QualityTypes.WEBDL720p, false },
            new object[] { QualityTypes.SDTV, false, QualityTypes.SDTV, true, QualityTypes.SDTV, true },
            new object[] { QualityTypes.WEBDL1080p, false, QualityTypes.WEBDL1080p, false, QualityTypes.WEBDL1080p, false }
        };

        [Test, TestCaseSource("IsUpgradeTestCases")]
        public void IsUpgradeTest(QualityTypes current, bool currentProper, QualityTypes newQuality, bool newProper, QualityTypes cutoff, bool expected)
        {
            new QualityUpgradeSpecification().IsSatisfiedBy(new QualityModel(current, currentProper), new QualityModel(newQuality, newProper), cutoff)
                    .Should().Be(expected);
        }
    }
}