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
    public void should_require_authentication_for_base()
    {
        new TmdbPersonSettings { PersonId = 1, AccountId = "a" }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_be_valid_when_authenticated_for_account()
    {
        new TmdbAccountSettings { AuthToken = "t", AccountId = "a" }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void should_require_account_id_for_account()
    {
        new TmdbAccountSettings { AuthToken = "t" }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_be_valid_with_list_id_for_list()
    {
        new TmdbListSettings { AuthToken = "t", ListId = 5 }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void should_be_valid_with_account_list_id_for_list()
    {
        new TmdbListSettings { AuthToken = "t", AccountListId = 5 }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void should_be_valid_with_list_id_and_account_list_none_for_list()
    {
        new TmdbListSettings { AuthToken = "t", ListId = 5, AccountListId = 0 }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void should_be_invalid_without_any_list_for_list()
    {
        new TmdbListSettings { AuthToken = "t" }.Validate().IsValid.Should().BeFalse();
        new TmdbListSettings { AuthToken = "t", AccountListId = 0 }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_be_invalid_with_both_list_id_and_account_list_id_for_list()
    {
        new TmdbListSettings { AuthToken = "t", ListId = 5, AccountListId = 6 }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_be_invalid_with_non_positive_list_id_for_list()
    {
        new TmdbListSettings { AuthToken = "t", ListId = 0 }.Validate().IsValid.Should().BeFalse();
        new TmdbListSettings { AuthToken = "t", ListId = -1 }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_be_valid_with_cast_credits_for_person()
    {
        new TmdbPersonSettings { AuthToken = "t", PersonId = 1 }.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void should_be_valid_with_only_crew_departments_for_person()
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
    public void should_be_invalid_without_cast_or_crew_for_person()
    {
        new TmdbPersonSettings { AuthToken = "t", PersonId = 1, IncludingCastCredits = false }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_be_invalid_without_person_id_for_person()
    {
        new TmdbPersonSettings { AuthToken = "t" }.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_be_valid_by_default_for_discover()
    {
        Discover().Validate().IsValid.Should().BeTrue();
    }

    [TestCase("0")]
    [TestCase("5.5")]
    [TestCase("10")]
    public void should_accept_valid_vote_average_for_discover(string value)
    {
        var settings = Discover();
        settings.VoteAverageMinimum = value;
        settings.Validate().IsValid.Should().BeTrue();
    }

    [TestCase("-1")]
    [TestCase("10.1")]
    [TestCase("abc")]
    public void should_reject_invalid_vote_average_for_discover(string value)
    {
        var settings = Discover();
        settings.VoteAverageMinimum = value;
        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_report_negative_vote_average_as_below_zero_for_discover()
    {
        var settings = Discover();
        settings.VoteAverageMinimum = "-1";
        settings.Validate().Errors.Should().ContainSingle().Which.ErrorMessage.Should().Contain("greater than or equal to zero");
    }

    [TestCase("0")]
    [TestCase("200")]
    public void should_accept_valid_vote_count_for_discover(string value)
    {
        var settings = Discover();
        settings.VoteCountMinimum = value;
        settings.Validate().IsValid.Should().BeTrue();
    }

    [TestCase("-1")]
    [TestCase("1.5")]
    [TestCase("abc")]
    public void should_reject_invalid_vote_count_for_discover(string value)
    {
        var settings = Discover();
        settings.VoteCountMinimum = value;
        settings.Validate().IsValid.Should().BeFalse();
    }

    [TestCase("1")]
    [TestCase("1,2,3")]
    [TestCase("1|2|3")]
    public void should_accept_delimited_keyword_and_company_ids_for_discover(string value)
    {
        var settings = Discover();
        settings.WithKeywords = value;
        settings.WithCompanies = value;
        settings.Validate().IsValid.Should().BeTrue();
    }

    [TestCase("a")]
    [TestCase("1,")]
    [TestCase("1;2")]
    public void should_reject_invalid_keyword_and_company_ids_for_discover(string value)
    {
        var keywords = Discover();
        keywords.WithKeywords = value;
        keywords.Validate().IsValid.Should().BeFalse();

        var companies = Discover();
        companies.WithCompanies = value;
        companies.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_reject_invalid_network_ids_for_discover()
    {
        var settings = Discover();
        settings.WithNetworks = ["213", "abc"];
        settings.Validate().IsValid.Should().BeFalse();

        settings.WithNetworks = ["0"];
        settings.Validate().IsValid.Should().BeFalse();

        settings.WithNetworks = [" 213"];
        settings.Validate().IsValid.Should().BeFalse();

        settings.WithNetworks = ["+213"];
        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_accept_valid_network_ids_for_discover()
    {
        var settings = Discover();
        settings.WithNetworks = ["213", "49"];
        settings.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void should_accept_air_date_day_offsets_for_discover()
    {
        var settings = Discover();
        settings.AirDateMinimumDays = -7;
        settings.AirDateMaximumDays = 7;
        settings.Validate().IsValid.Should().BeTrue();
    }

    [Test]
    public void should_reject_maximum_before_minimum_for_discover()
    {
        var settings = Discover();
        settings.AirDateMinimumDays = 7;
        settings.AirDateMaximumDays = -7;
        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_reject_out_of_range_day_offsets_for_discover()
    {
        var settings = Discover();
        settings.AirDateMinimumDays = int.MinValue;
        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_be_explicit_and_stable_for_language_values()
    {
        ((int)TmdbLanguage.Any).Should().Be(0);
        ((int)TmdbLanguage.Ab).Should().Be(1);
        ((int)TmdbLanguage.Zu).Should().Be(187);
    }

    [Test]
    public void should_be_unique_for_language_values()
    {
        var values = (int[])Enum.GetValues(typeof(TmdbLanguage));

        values.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void should_be_stable_for_department_values()
    {
        ((int)TmdbDepartmentType.Art).Should().Be(0);
        ((int)TmdbDepartmentType.CostumeMakeup).Should().Be(2);
        ((int)TmdbDepartmentType.Writing).Should().Be(10);
    }

    [TestCase("NaN")]
    [TestCase("Infinity")]
    [TestCase("1e1")]
    public void should_reject_non_decimal_vote_average_for_discover(string value)
    {
        var settings = Discover();
        settings.VoteAverageMinimum = value;
        settings.Validate().Errors.Select(e => e.PropertyName).Should().Contain(nameof(TmdbDiscoverSettings.VoteAverageMinimum));
    }

    [Test]
    public void should_report_failing_property_for_validation()
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
    public void should_not_throw_when_department_selection_is_null_for_person()
    {
        var settings = new TmdbPersonSettings { AuthToken = "t", PersonId = 1, IncludingCastCredits = false, IncludeDepartmentTypes = null };

        settings.Validate().IsValid.Should().BeFalse();
    }

    [Test]
    public void should_match_lowercase_enum_name_for_language_hint()
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
    public void should_be_sequential_in_declaration_order_for_language_values()
    {
        var values = Enum.GetValues<TmdbLanguage>().Select(l => (int)l).ToArray();

        values.Should().Equal(Enumerable.Range(0, values.Length));
    }

    [TestCase("https://api.themoviedb.org")]
    [TestCase("https://api.themoviedb.org/")]
    [TestCase("HTTPS://API.THEMOVIEDB.ORG")]
    public void should_accept_tmdb_base_url_for_base(string baseUrl)
    {
        new TmdbPersonSettings { AuthToken = "t", PersonId = 1, BaseUrl = baseUrl }.Validate().IsValid.Should().BeTrue();
    }

    [TestCase("https://evil.example.com")]
    [TestCase("http://api.themoviedb.org")]
    [TestCase("https://api.themoviedb.org.evil.example.com")]
    [TestCase("not a url")]
    public void should_reject_other_base_urls_for_base(string baseUrl)
    {
        new TmdbPersonSettings { AuthToken = "t", PersonId = 1, BaseUrl = baseUrl }.Validate().Errors
            .Select(e => e.PropertyName).Should().Contain(nameof(TmdbPersonSettings.BaseUrl));
    }

    private static TmdbDiscoverSettings Discover() => new() { AuthToken = "t" };
}
