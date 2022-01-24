using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class CustomFormatAllowedByProfileSpecificationFixture : CoreTest<CustomFormatAllowedbyProfileSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        private CustomFormat _format1;
        private CustomFormat _format2;

        [SetUp]
        public void Setup()
        {
            _format1 = new CustomFormat("Awesome Format");
            _format1.Id = 1;

            _format2 = new CustomFormat("Cool Format");
            _format2.Id = 2;

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.QualityProfile = new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    MinFormatScore = 1
                })
                .Build();

            _remoteEpisode = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
            };

            CustomFormatsFixture.GivenCustomFormats(_format1, _format2);
        }

        [Test]
        public void should_allow_if_format_score_greater_than_min()
        {
            _remoteEpisode.CustomFormats = new List<CustomFormat> { _format1 };
            _remoteEpisode.Series.QualityProfile.Value.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);
            _remoteEpisode.CustomFormatScore = _remoteEpisode.Series.QualityProfile.Value.CalculateCustomFormatScore(_remoteEpisode.CustomFormats);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_deny_if_format_score_not_greater_than_min()
        {
            _remoteEpisode.CustomFormats = new List<CustomFormat> { _format2 };
            _remoteEpisode.Series.QualityProfile.Value.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);
            _remoteEpisode.CustomFormatScore = _remoteEpisode.Series.QualityProfile.Value.CalculateCustomFormatScore(_remoteEpisode.CustomFormats);

            Console.WriteLine(_remoteEpisode.CustomFormatScore);
            Console.WriteLine(_remoteEpisode.Series.QualityProfile.Value.MinFormatScore);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_deny_if_format_score_not_greater_than_min_2()
        {
            _remoteEpisode.CustomFormats = new List<CustomFormat> { _format2, _format1 };
            _remoteEpisode.Series.QualityProfile.Value.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);
            _remoteEpisode.CustomFormatScore = _remoteEpisode.Series.QualityProfile.Value.CalculateCustomFormatScore(_remoteEpisode.CustomFormats);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_allow_if_all_format_is_defined_in_profile()
        {
            _remoteEpisode.CustomFormats = new List<CustomFormat> { _format2, _format1 };
            _remoteEpisode.Series.QualityProfile.Value.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteEpisode.CustomFormatScore = _remoteEpisode.Series.QualityProfile.Value.CalculateCustomFormatScore(_remoteEpisode.CustomFormats);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_deny_if_no_format_was_parsed_and_min_score_positive()
        {
            _remoteEpisode.CustomFormats = new List<CustomFormat> { };
            _remoteEpisode.Series.QualityProfile.Value.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteEpisode.CustomFormatScore = _remoteEpisode.Series.QualityProfile.Value.CalculateCustomFormatScore(_remoteEpisode.CustomFormats);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_allow_if_no_format_was_parsed_min_score_is_zero()
        {
            _remoteEpisode.CustomFormats = new List<CustomFormat> { };
            _remoteEpisode.Series.QualityProfile.Value.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteEpisode.Series.QualityProfile.Value.MinFormatScore = 0;
            _remoteEpisode.CustomFormatScore = _remoteEpisode.Series.QualityProfile.Value.CalculateCustomFormatScore(_remoteEpisode.CustomFormats);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }
    }
}
