using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.Trakt;
using NzbDrone.Core.ImportLists.Trakt.List;
using NzbDrone.Core.ImportLists.Trakt.Popular;
using NzbDrone.Core.ImportLists.Trakt.User;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Trakt
{
    [TestFixture]
    public class TraktValidationFixture : CoreTest
    {
        [TestCase("0-100")]
        [TestCase("50-50")]
        [TestCase("100-100")]
        public void should_accept_supported_rating_ranges(string rating)
        {
            CreateValidListSettings(rating: rating).Validate().IsValid.Should().BeTrue();
            CreateValidPopularSettings(rating: rating).Validate().IsValid.Should().BeTrue();
            CreateValidUserSettings(rating: rating).Validate().IsValid.Should().BeTrue();
        }

        [TestCase("10")]
        [TestCase("10-5")]
        [TestCase("00-10")]
        [TestCase("10-101")]
        [TestCase("1000-1000")]
        [TestCase("10 - 20")]
        public void should_reject_invalid_rating_ranges(string rating)
        {
            CreateValidListSettings(rating: rating).Validate().IsValid.Should().BeFalse();
            CreateValidPopularSettings(rating: rating).Validate().IsValid.Should().BeFalse();
            CreateValidUserSettings(rating: rating).Validate().IsValid.Should().BeFalse();
        }

        [TestCase("1990")]
        [TestCase("1990-2000")]
        public void should_accept_supported_year_filters(string years)
        {
            CreateValidListSettings(years: years).Validate().IsValid.Should().BeTrue();
            CreateValidPopularSettings(years: years).Validate().IsValid.Should().BeTrue();
            CreateValidUserSettings(years: years).Validate().IsValid.Should().BeTrue();
        }

        [TestCase("234923498237423-234723477")]
        [TestCase("199")]
        [TestCase("1990-1980")]
        [TestCase("1990 - 2000")]
        public void should_reject_invalid_year_filters(string years)
        {
            CreateValidListSettings(years: years).Validate().IsValid.Should().BeFalse();
            CreateValidPopularSettings(years: years).Validate().IsValid.Should().BeFalse();
            CreateValidUserSettings(years: years).Validate().IsValid.Should().BeFalse();
        }

        [TestCase("genres=comedy")]
        [TestCase("ratings=80-100")]
        [TestCase("years=1990-2000")]
        [TestCase("limit=10")]
        public void should_reject_reserved_additional_parameters(string additionalParameters)
        {
            CreateValidListSettings(additionalParameters: additionalParameters).Validate().IsValid.Should().BeFalse();
            CreateValidPopularSettings(additionalParameters: additionalParameters).Validate().IsValid.Should().BeFalse();
            CreateValidUserSettings(additionalParameters: additionalParameters).Validate().IsValid.Should().BeFalse();
        }

        [Test]
        public void should_allow_non_reserved_additional_parameters()
        {
            CreateValidListSettings(additionalParameters: "languages=en").Validate().IsValid.Should().BeTrue();
            CreateValidPopularSettings(additionalParameters: "languages=en").Validate().IsValid.Should().BeTrue();
            CreateValidUserSettings(additionalParameters: "languages=en").Validate().IsValid.Should().BeTrue();
        }

        [Test]
        public void should_ignore_reserved_additional_parameters_when_building_filter_parameters()
        {
            var parameters = TraktQueryHelper.BuildFilterParameters(
                "80-100",
                "Drama",
                "1990-2000",
                25,
                "genres=comedy&ratings=10-20&years=2000-2010&limit=10&languages=en");

            parameters.Should().ContainKey("genres").WhoseValue.Should().Be("drama");
            parameters.Should().ContainKey("ratings").WhoseValue.Should().Be("80-100");
            parameters.Should().ContainKey("years").WhoseValue.Should().Be("1990-2000");
            parameters.Should().ContainKey("limit").WhoseValue.Should().Be("25");
            parameters.Should().ContainKey("languages").WhoseValue.Should().Be("en");
            parameters.Should().HaveCount(5);
        }

        private static TraktListSettings CreateValidListSettings(string rating = "80-100", string years = "1990-2000", string additionalParameters = "languages=en")
        {
            return new TraktListSettings
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                Expires = DateTime.UtcNow.AddDays(1),
                Username = "sonarr",
                Listname = "watchlist",
                Rating = rating,
                Years = years,
                TraktAdditionalParameters = additionalParameters
            };
        }

        private static TraktPopularSettings CreateValidPopularSettings(string rating = "80-100", string years = "1990-2000", string additionalParameters = "languages=en")
        {
            return new TraktPopularSettings
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                Expires = DateTime.UtcNow.AddDays(1),
                Rating = rating,
                Years = years,
                TraktAdditionalParameters = additionalParameters
            };
        }

        private static TraktUserSettings CreateValidUserSettings(string rating = "80-100", string years = "1990-2000", string additionalParameters = "languages=en")
        {
            return new TraktUserSettings
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                Expires = DateTime.UtcNow.AddDays(1),
                AuthUser = "sonarr-user",
                Rating = rating,
                Years = years,
                TraktAdditionalParameters = additionalParameters
            };
        }
    }
}