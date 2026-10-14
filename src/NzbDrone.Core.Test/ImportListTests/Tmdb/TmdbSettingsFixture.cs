using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ImportLists.Tmdb;
using NzbDrone.Core.ImportLists.Tmdb.Account;
using NzbDrone.Core.ImportLists.Tmdb.Discover;
using NzbDrone.Core.ImportLists.Tmdb.List;
using NzbDrone.Core.ImportLists.Tmdb.Person;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Tmdb;

[TestFixture]
public class TmdbSettingsFixture : CoreTest
{
    [Test]
    public void BaseShouldRequireAuthentication()
    {
        new TmdbPersonSettings { PersonId = 1, AccountId = "a" }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void AccountShouldBeValidWhenAuthenticated()
    {
        new TmdbAccountSettings { AuthToken = "t", AccountId = "a" }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void AccountShouldRequireAccountId()
    {
        new TmdbAccountSettings { AuthToken = "t" }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void ListShouldBeValidWithListId()
    {
        new TmdbListSettings { AuthToken = "t", ListId = 5 }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void ListShouldBeValidWithAccountListId()
    {
        new TmdbListSettings { AuthToken = "t", AccountListId = 5 }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void ListShouldBeValidWithListIdAndAccountListNone()
    {
        new TmdbListSettings { AuthToken = "t", ListId = 5, AccountListId = 0 }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void ListShouldBeInvalidWithoutAnyList()
    {
        new TmdbListSettings { AuthToken = "t" }.Validate().IsValid.Should().BeFalse();
        new TmdbListSettings { AuthToken = "t", AccountListId = 0 }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void ListShouldBeInvalidWithBothListIdAndAccountListId()
    {
        new TmdbListSettings { AuthToken = "t", ListId = 5, AccountListId = 6 }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void ListShouldBeInvalidWithNonPositiveListId()
    {
        new TmdbListSettings { AuthToken = "t", ListId = 0 }.Validate().IsValid.Should().BeFalse();
        new TmdbListSettings { AuthToken = "t", ListId = -1 }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void PersonShouldBeValidWithCastCredits()
    {
        new TmdbPersonSettings { AuthToken = "t", PersonId = 1 }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void PersonShouldBeValidWithOnlyCrewDepartments()
    {
        new TmdbPersonSettings
        {
            AuthToken = "t",
            PersonId = 1,
            IncludingCastCredits = false,
            IncludeDepartmentTypes = [(int)TmdbDepartmentType.Directing]
        }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void PersonShouldBeInvalidWithoutCastOrCrew()
    {
        new TmdbPersonSettings { AuthToken = "t", PersonId = 1, IncludingCastCredits = false }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void PersonShouldBeInvalidWithoutPersonId()
    {
        new TmdbPersonSettings { AuthToken = "t" }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void DiscoverShouldBeValidByDefault()
    {
        Discover().Validate().IsValid.Should().BeTrue();
    }

    [TestCase("0")]
    [TestCase("5.5")]
    [TestCase("10")]
    public void DiscoverShouldAcceptValidVoteAverage(string value)
    {
        var settings = Discover();
        settings.VoteAverageMinimum = value;
        settings.Validate().IsValid.Should().BeTrue();
    }

    [TestCase("-1")]
    [TestCase("10.1")]
    [TestCase("abc")]
    public void DiscoverShouldRejectInvalidVoteAverage(string value)
    {
        var settings = Discover();
        settings.VoteAverageMinimum = value;
        settings.Validate().IsValid.Should().BeFalse();
    }

    [TestCase("0")]
    [TestCase("200")]
    public void DiscoverShouldAcceptValidVoteCount(string value)
    {
        var settings = Discover();
        settings.VoteCountMinimum = value;
        settings.Validate().IsValid.Should().BeTrue();
    }

    [TestCase("-1")]
    [TestCase("1.5")]
    [TestCase("abc")]
    public void DiscoverShouldRejectInvalidVoteCount(string value)
    {
        var settings = Discover();
        settings.VoteCountMinimum = value;
        settings.Validate().IsValid.Should().BeFalse();
    }

    [TestCase("1")]
    [TestCase("1,2,3")]
    [TestCase("1|2|3")]
    public void DiscoverShouldAcceptDelimitedKeywordAndCompanyIds(string value)
    {
        var settings = Discover();
        settings.WithKeywords = value;
        settings.WithCompanies = value;
        settings.Validate().IsValid.Should().BeTrue();
    }

    [TestCase("a")]
    [TestCase("1,")]
    [TestCase("1;2")]
    public void DiscoverShouldRejectInvalidKeywordAndCompanyIds(string value)
    {
        var keywords = Discover();
        keywords.WithKeywords = value;
        keywords.Validate().IsValid.Should().BeFalse();

        var companies = Discover();
        companies.WithCompanies = value;
        companies.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void DiscoverShouldRejectInvalidNetworkIds()
    {
        var settings = Discover();
        settings.WithNetworks = ["213", "abc"];
        settings.Validate().IsValid.Should().BeFalse();

        settings.WithNetworks = ["0"];
        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void DiscoverShouldAcceptValidNetworkIds()
    {
        var settings = Discover();
        settings.WithNetworks = ["213", "49"];
        settings.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void DiscoverShouldAcceptAirDateDayOffsets()
    {
        var settings = Discover();
        settings.AirDateMinimumDays = -7;
        settings.AirDateMaximumDays = 7;
        settings.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void DiscoverShouldRejectMaximumBeforeMinimum()
    {
        var settings = Discover();
        settings.AirDateMinimumDays = 7;
        settings.AirDateMaximumDays = -7;
        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void DiscoverShouldRejectOutOfRangeDayOffsets()
    {
        var settings = Discover();
        settings.AirDateMinimumDays = int.MinValue;
        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void LanguageValuesShouldBeExplicitAndStable()
    {
        ((int)TmdbLanguage.Any).Should().Be(0);
        ((int)TmdbLanguage.Ab).Should().Be(1);
        ((int)TmdbLanguage.Zu).Should().Be(187);
    }

    [Test]
    public void LanguageValuesShouldBeUnique()
    {
        var values = (int[])Enum.GetValues(typeof(TmdbLanguage));

        values.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void DepartmentValuesShouldBeStable()
    {
        ((int)TmdbDepartmentType.Art).Should().Be(0);
        ((int)TmdbDepartmentType.CostumeMakeup).Should().Be(2);
        ((int)TmdbDepartmentType.Writing).Should().Be(10);
    }

    [TestCase("NaN")]
    [TestCase("Infinity")]
    [TestCase("1e1")]
    public void DiscoverShouldRejectNonDecimalVoteAverage(string value)
    {
        var settings = Discover();
        settings.VoteAverageMinimum = value;
        settings.Validate().Errors.Select(e => e.PropertyName).Should().Contain(nameof(TmdbDiscoverSettings.VoteAverageMinimum));
    }

    [Test]
    public void ValidationShouldReportFailingProperty()
    {
        new TmdbPersonSettings { AuthToken = "t" }.Validate().Errors.Select(e => e.PropertyName)
            .Should().Equal(nameof(TmdbPersonSettings.PersonId));

        new TmdbPersonSettings { PersonId = 1, AccountId = "a" }.Validate().Errors.Select(e => e.PropertyName)
            .Should().Equal("SignIn");

        new TmdbAccountSettings { AuthToken = "t" }.Validate().Errors.Select(e => e.PropertyName)
            .Should().Equal("SignIn");

        var discover = Discover();
        discover.AirDateMinimumDays = 7;
        discover.AirDateMaximumDays = -7;
        discover.Validate().Errors.Select(e => e.PropertyName).Should().Equal(nameof(TmdbDiscoverSettings.AirDateMaximumDays));
    }

    [Test]
    public void PersonShouldNotThrowWhenDepartmentSelectionIsNull()
    {
        var settings = new TmdbPersonSettings { AuthToken = "t", PersonId = 1, IncludingCastCredits = false, IncludeDepartmentTypes = null };

        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void LanguageHintShouldMatchLowercaseEnumName()
    {
        foreach (var language in Enum.GetValues<TmdbLanguage>().Where(l => l != TmdbLanguage.Any))
        {
            var attribute = typeof(TmdbLanguage).GetField(language.ToString())!
                .GetCustomAttributes(typeof(FieldOptionAttribute), false)
                .Cast<FieldOptionAttribute>()
                .Single();

            attribute.Hint.Should().BeEquivalentTo(language.ToString(), language.ToString());
            attribute.Hint.Should().NotMatchRegex("[A-Z]", language.ToString());
        }
    }

    [Test]
    public void LanguageValuesShouldBeSequentialInDeclarationOrder()
    {
        var values = Enum.GetValues<TmdbLanguage>().Select(l => (int)l).ToArray();

        values.Should().Equal(Enumerable.Range(0, values.Length));
    }

    [TestCase("https://api.themoviedb.org")]
    [TestCase("https://api.themoviedb.org/")]
    [TestCase("HTTPS://API.THEMOVIEDB.ORG")]
    public void BaseShouldAcceptTmdbBaseUrl(string baseUrl)
    {
        new TmdbPersonSettings { AuthToken = "t", PersonId = 1, BaseUrl = baseUrl }.Validate().IsValid.Should().BeTrue();
    }

    [TestCase("https://evil.example.com")]
    [TestCase("http://api.themoviedb.org")]
    [TestCase("https://api.themoviedb.org.evil.example.com")]
    [TestCase("not a url")]
    public void BaseShouldRejectOtherBaseUrls(string baseUrl)
    {
        new TmdbPersonSettings { AuthToken = "t", PersonId = 1, BaseUrl = baseUrl }.Validate().Errors
            .Select(e => e.PropertyName).Should().Contain(nameof(TmdbPersonSettings.BaseUrl));
    }

    private static TmdbDiscoverSettings Discover() => new() { AuthToken = "t" };
}
