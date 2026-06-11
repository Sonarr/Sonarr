using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class SeriesEditionsFixture
    {
        [Test]
        public void should_normalize_missing_series_edition_to_standard()
        {
            SeriesEditions.Normalize(null).Should().Be(SeriesEditions.Standard);
            SeriesEditions.Normalize("").Should().Be(SeriesEditions.Standard);
        }

        [Test]
        public void should_return_rezero_directors_cut_aliases()
        {
            var series = new Series
            {
                TvdbId = 305089,
                SeriesEdition = SeriesEditions.DirectorsCut
            };

            var aliases = SeriesEditions.GetSearchAliases(series);

            aliases.Should().Contain("Re Zero Director's Cut");
            aliases.Should().Contain("Re Zero Shin Henshuu-ban");
            aliases.Should().Contain("Re Zero New Edit Version");
        }

        [Test]
        public void should_return_generic_directors_cut_aliases_for_any_series()
        {
            var series = new Series
            {
                TvdbId = 1,
                Title = "Some Show",
                SeriesEdition = SeriesEditions.DirectorsCut
            };

            var aliases = SeriesEditions.GetSearchAliases(series);

            aliases.Should().Contain("Some Show Director's Cut");
            aliases.Should().Contain("Some Show Directors Cut");
            aliases.Should().Contain("Some Show New Edit Version");
        }

        [Test]
        public void should_append_generic_directors_cut_display_suffix_for_any_series()
        {
            var series = new Series
            {
                TvdbId = 1,
                Title = "Some Show",
                SeriesEdition = SeriesEditions.DirectorsCut
            };

            SeriesEditions.GetDisplayTitle(series).Should().Be("Some Show [Director's Cut]");
        }

        [Test]
        public void should_filter_rezero_directors_cut_season_one_to_13_episodes()
        {
            var series = new Series
            {
                TvdbId = 305089,
                SeriesEdition = SeriesEditions.DirectorsCut
            };

            var episodes = Enumerable.Range(1, 25)
                                     .Select(i => new Episode { SeasonNumber = 1, EpisodeNumber = i })
                                     .ToList();

            SeriesEditions.ApplyEpisodeOverrides(series, episodes).Should().HaveCount(13);
        }

        [Test]
        public void should_map_rezero_directors_cut_titles_to_combined_episodes()
        {
            var series = new Series
            {
                TvdbId = 305089,
                SeriesEdition = SeriesEditions.DirectorsCut
            };

            var episodes = Enumerable.Range(1, 25)
                                     .Select(i => new Episode
                                     {
                                         SeasonNumber = 1,
                                         EpisodeNumber = i,
                                         Title = $"Episode {i}",
                                         Runtime = 25
                                     })
                                     .ToList();

            var mappedEpisodes = SeriesEditions.ApplyEpisodeOverrides(series, episodes);

            mappedEpisodes.Should().HaveCount(13);
            mappedEpisodes.First(e => e.EpisodeNumber == 2).Title.Should().Be("Reunion with the Witch \\ Starting Life from Zero in Another World");
            mappedEpisodes.First(e => e.EpisodeNumber == 13).Title.Should().Be("The Self-Proclaimed Knight and the Greatest Knight \\ That's All This Story Is About");
            mappedEpisodes.First(e => e.EpisodeNumber == 13).Runtime.Should().Be(50);
            mappedEpisodes.First(e => e.EpisodeNumber == 13).AirDate.Should().Be("2020-04-01");
        }

        [Test]
        public void should_not_filter_standard_rezero_episodes()
        {
            var series = new Series
            {
                TvdbId = 305089,
                SeriesEdition = SeriesEditions.Standard
            };

            var episodes = Enumerable.Range(1, 25)
                                     .Select(i => new Episode { SeasonNumber = 1, EpisodeNumber = i })
                                     .ToList();

            SeriesEditions.ApplyEpisodeOverrides(series, episodes).Should().HaveCount(25);
        }

        [Test]
        public void should_not_apply_episode_overrides_to_generic_directors_cut_without_profile()
        {
            var series = new Series
            {
                TvdbId = 1,
                SeriesEdition = SeriesEditions.DirectorsCut
            };

            var episodes = Enumerable.Range(1, 25)
                                     .Select(i => new Episode { SeasonNumber = 1, EpisodeNumber = i })
                                     .ToList();

            SeriesEditions.ApplyEpisodeOverrides(series, episodes).Should().HaveCount(25);
        }
    }
}
