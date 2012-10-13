// ReSharper disable RedundantUsingDirective

using System.Linq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QualityUpgradeSpecificationFixture : CoreTest
    {

        [TestCase(QualityTypes.SDTV, false, QualityTypes.SDTV, true, QualityTypes.SDTV, Result = true)]
        [TestCase(QualityTypes.WEBDL, false, QualityTypes.WEBDL, true, QualityTypes.WEBDL, Result = true)]
        [TestCase(QualityTypes.SDTV, false, QualityTypes.SDTV, false, QualityTypes.SDTV, Result = false)]
        [TestCase(QualityTypes.SDTV, false, QualityTypes.DVD, true, QualityTypes.SDTV, Result = false)]
        [TestCase(QualityTypes.WEBDL, false, QualityTypes.HDTV, true, QualityTypes.Bluray720p, Result = false)]
        [TestCase(QualityTypes.WEBDL, false, QualityTypes.HDTV, true, QualityTypes.WEBDL, Result = false)]
        [TestCase(QualityTypes.WEBDL, false, QualityTypes.WEBDL, false, QualityTypes.WEBDL, Result = false)]
        [TestCase(QualityTypes.SDTV, false, QualityTypes.SDTV, true, QualityTypes.SDTV, Result = true)]
        public bool IsUpgradeTest(QualityTypes current, bool currentProper, QualityTypes newQuality, bool newProper, QualityTypes cutoff)
        {
            return new QualityUpgradeSpecification().IsSatisfiedBy(new QualityModel(current, currentProper), new QualityModel(newQuality, newProper), cutoff);
        }
    }
}