// ReSharper disable RedundantUsingDirective

using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QualityAllowedByProfileSpecificationFixture : SqlCeTest
    {
        private QualityAllowedByProfileSpecification _qualityAllowedByProfile;
        private EpisodeParseResult parseResult;

        public static object[] AllowedTestCases =
        {
            new object[] { QualityTypes.DVD },
            new object[] { QualityTypes.HDTV720p },
            new object[] { QualityTypes.Bluray1080p }
        };

        public static object[] DeniedTestCases =
        {
            new object[] { QualityTypes.SDTV },
            new object[] { QualityTypes.WEBDL720p },
            new object[] { QualityTypes.Bluray720p }
        };

        [SetUp]
        public void Setup()
        {
            _qualityAllowedByProfile = Mocker.Resolve<QualityAllowedByProfileSpecification>();

            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = QualityTypes.Bluray1080p })
                         .Build();

            parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                Quality = new QualityModel(QualityTypes.DVD, true),
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
            };
        }

        [Test, TestCaseSource("AllowedTestCases")]
        public void should_allow_if_quality_is_defined_in_profile(QualityTypes qualityType)
        {
            parseResult.Quality.Quality = qualityType;
            parseResult.Series.QualityProfile.Allowed = new List<QualityTypes> { QualityTypes.DVD, QualityTypes.HDTV720p, QualityTypes.Bluray1080p };

            _qualityAllowedByProfile.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test, TestCaseSource("DeniedTestCases")]
        public void should_not_allow_if_quality_is_not_defined_in_profile(QualityTypes qualityType)
        {
            parseResult.Quality.Quality = qualityType;
            parseResult.Series.QualityProfile.Allowed = new List<QualityTypes> { QualityTypes.DVD, QualityTypes.HDTV720p, QualityTypes.Bluray1080p };

            _qualityAllowedByProfile.IsSatisfiedBy(parseResult).Should().BeFalse();
        }
    }
}