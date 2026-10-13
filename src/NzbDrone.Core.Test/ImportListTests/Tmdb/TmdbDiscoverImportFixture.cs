using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.Tmdb.Discover;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Tmdb;

[TestFixture]
public class TmdbDiscoverImportFixture : CoreTest<TmdbDiscoverImport>
{
    [SetUp]
    public void Setup()
    {
        Mocker.GetMock<ISonarrCloudRequestBuilder>()
            .SetupGet(s => s.Services)
            .Returns(new HttpRequestBuilder("https://services.sonarr.tv/v1/").CreateFactory());
    }

    [Test]
    public void PresetsShouldBeValidAndNamedWithoutDates()
    {
        var presets = Subject.DefaultDefinitions.ToArray();

        presets.Select(p => p.Name).Should().Equal("Airing Today", "On The Air", "Top Rated");

        foreach (var preset in presets)
        {
            var settings = (TmdbDiscoverSettings)preset.Settings;
            settings.AuthToken = "token";
            settings.Validate().IsValid.Should().BeTrue(preset.Name);
        }
    }

    [Test]
    public void OnTheAirPresetShouldResolveDatesRelativeToToday()
    {
        var preset = (TmdbDiscoverSettings)Subject.DefaultDefinitions.Single(p => p.Name == "On The Air").Settings;
        preset.AuthToken = "token";

        var request = new TmdbDiscoverRequestGenerator(preset, 1).GetListItems().GetAllTiers().SelectMany(t => t).Single();

        request.Url.FullUri.Should().Contain($"air_date.gte={DateTime.UtcNow.Date:yyyy-MM-dd}");
        request.Url.FullUri.Should().Contain($"air_date.lte={DateTime.UtcNow.Date.AddDays(7):yyyy-MM-dd}");
    }
}
