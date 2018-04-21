using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ValidateParsedEpisodeInfoFixture : CoreTest
    {
        private ParsedEpisodeInfo _parsedEpisodeInfo;
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _parsedEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew()
                                                           .With(p => p.AirDate = null)
                                                           .Build();

            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
                                     .Build();
        }

        private void GivenDailyParsedEpisodeInfo()
        {
            _parsedEpisodeInfo.AirDate = "2018-05-21";
        }

        private void GivenDailySeries()
        {
            _series.SeriesType = SeriesTypes.Daily;
        }

        [Test]
        public void should_return_true_if_episode_info_is_not_daily()
        {
            ValidateParsedEpisodeInfo.ValidateForSeriesType(_parsedEpisodeInfo, _series).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_info_is_daily_for_daily_series()
        {
            GivenDailyParsedEpisodeInfo();
            GivenDailySeries();

            ValidateParsedEpisodeInfo.ValidateForSeriesType(_parsedEpisodeInfo, _series).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_episode_info_is_daily_for_standard_series()
        {
            GivenDailyParsedEpisodeInfo();

            ValidateParsedEpisodeInfo.ValidateForSeriesType(_parsedEpisodeInfo, _series).Should().BeFalse();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_log_warning_if_warnIfInvalid_is_false()
        {
            GivenDailyParsedEpisodeInfo();

            ValidateParsedEpisodeInfo.ValidateForSeriesType(_parsedEpisodeInfo, _series, false);
            ExceptionVerification.ExpectedWarns(0);
        }
    }
}
