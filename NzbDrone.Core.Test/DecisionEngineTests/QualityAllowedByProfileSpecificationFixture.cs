

using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.DecisionEngine;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    
    public class QualityAllowedByProfileSpecificationFixture : CoreTest
    {
        private QualityAllowedByProfileSpecification _qualityAllowedByProfile;
        private EpisodeParseResult parseResult;

        public static object[] AllowedTestCases =
        {
            new object[] { Quality.DVD },
            new object[] { Quality.HDTV720p },
            new object[] { Quality.Bluray1080p }
        };

        public static object[] DeniedTestCases =
        {
            new object[] { Quality.SDTV },
            new object[] { Quality.WEBDL720p },
            new object[] { Quality.Bluray720p }
        };

        [SetUp]
        public void Setup()
        {
            _qualityAllowedByProfile = Mocker.Resolve<QualityAllowedByProfileSpecification>();

            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p })
                         .Build();

            parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                Quality = new QualityModel(Quality.DVD, true),
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
            };
        }

        [Test, TestCaseSource("AllowedTestCases")]
        public void should_allow_if_quality_is_defined_in_profile(Quality qualityType)
        {
            parseResult.Quality.Quality = qualityType;
            parseResult.Series.QualityProfile.Allowed = new List<Quality> { Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p };

            _qualityAllowedByProfile.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test, TestCaseSource("DeniedTestCases")]
        public void should_not_allow_if_quality_is_not_defined_in_profile(Quality qualityType)
        {
            parseResult.Quality.Quality = qualityType;
            parseResult.Series.QualityProfile.Allowed = new List<Quality> { Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p };

            _qualityAllowedByProfile.IsSatisfiedBy(parseResult).Should().BeFalse();
        }
    }
}