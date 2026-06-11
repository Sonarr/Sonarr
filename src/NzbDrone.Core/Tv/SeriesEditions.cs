using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Tv
{
    public static class SeriesEditions
    {
        public const string Standard = "standard";
        public const string DirectorsCut = "directors_cut";
        public const string Custom = "custom";

        private static readonly HashSet<string> ValidEditions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Standard,
            DirectorsCut,
            Custom
        };

        private static readonly Regex DirectorsCutMarker = new Regex(
            @"\b(director'?s?\s*cut|shin\s*henshuu[-\s]*ban|shin\s*henshuuban|new\s*edit\s*version|new\s*edition|2020)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly IReadOnlyList<EditionProfile> EditionProfiles = new List<EditionProfile>
        {
            new EditionProfile
            {
                TvdbId = 305089,
                SeriesEdition = DirectorsCut,
                DisplayTitle = "Re:ZERO -Starting Life in Another World- [Director's Cut]",
                SearchAliases = new List<string>
                {
                    "Re Zero Director's Cut",
                    "Re Zero Directors Cut",
                    "Re Zero Shin Henshuu-ban",
                    "Re Zero Shin Henshuuban",
                    "Re Zero New Edit Version",
                    "Re Zero New Edition",
                    "Re Zero 2020"
                },
                RequiredMarker = DirectorsCutMarker,
                EpisodeMaps = new List<EditionEpisodeMap>
                {
                    new(1, 1, 1, 1, "The End of the Beginning and the Beginning of the End", "2020-01-01", 53),
                    new(1, 2, 1, 2, "Reunion with the Witch \\ Starting Life from Zero in Another World", "2020-01-08", 50),
                    new(1, 3, 1, 4, "The Happy Roswaal Mansion Family \\ The Morning of Our Promise Is Still Distant", "2020-01-15", 50),
                    new(1, 4, 1, 6, "The Sound of Chains \\ Natsuki Subaru's Restart", "2020-01-22", 50),
                    new(1, 5, 1, 8, "I Cried, Cried My Lungs Out, and Stopped Crying \\ The Meaning of Courage", "2020-01-29", 50),
                    new(1, 6, 1, 10, "Fanatical Methods Like a Demon \\ Rem", "2020-02-05", 50),
                    new(1, 7, 1, 12, "Return to the Capital \\ Self-Proclaimed Knight Natsuki Subaru", "2020-02-19", 50),
                    new(1, 8, 1, 14, "The Sickness Called Despair \\ The Outside of Madness", "2020-02-26", 50),
                    new(1, 9, 1, 16, "The Greed of a Pig \\ Disgrace in the Extreme", "2020-03-04", 50),
                    new(1, 10, 1, 18, "From Zero \\ Battle Against the White Whale", "2020-03-11", 50),
                    new(1, 11, 1, 20, "Wilhelm van Astrea \\ A Wager that Defies Despair", "2020-03-18", 50),
                    new(1, 12, 1, 22, "A Flash of Sloth \\ Nefarious Sloth", "2020-03-25", 50),
                    new(1, 13, 1, 24, "The Self-Proclaimed Knight and the Greatest Knight \\ That's All This Story Is About", "2020-04-01", 50, "season")
                }
            }
        };

        public static string Normalize(string seriesEdition)
        {
            if (seriesEdition.IsNullOrWhiteSpace())
            {
                return Standard;
            }

            var normalized = seriesEdition.Trim().ToLowerInvariant();

            return IsValid(normalized) ? normalized : seriesEdition;
        }

        public static bool IsValid(string seriesEdition)
        {
            return ValidEditions.Contains(NormalizeKnownValue(seriesEdition));
        }

        public static bool IsStandard(string seriesEdition)
        {
            return Normalize(seriesEdition).Equals(Standard, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetDisplayTitle(Series series)
        {
            var profile = GetEditionProfile(series);

            if (profile?.DisplayTitle.IsNotNullOrWhiteSpace() == true)
            {
                return profile.DisplayTitle;
            }

            var edition = Normalize(series.SeriesEdition);

            if (edition.Equals(DirectorsCut, StringComparison.OrdinalIgnoreCase))
            {
                return AppendEditionSuffix(series.Title, "Director's Cut");
            }

            if (edition.Equals(Custom, StringComparison.OrdinalIgnoreCase))
            {
                return AppendEditionSuffix(series.Title, "Custom");
            }

            return series.Title;
        }

        public static string GetTitleSlug(Series series)
        {
            if (!IsStandard(series.SeriesEdition) && series.TitleSlug.IsNotNullOrWhiteSpace())
            {
                var slugEdition = Normalize(series.SeriesEdition).Replace("_", "-");
                var suffix = $"-{slugEdition}";

                if (!series.TitleSlug.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return $"{series.TitleSlug}{suffix}";
                }
            }

            return series.TitleSlug;
        }

        public static List<string> GetSearchAliases(Series series)
        {
            var aliases = new List<string>();
            var profile = GetEditionProfile(series);

            if (profile?.SearchAliases != null)
            {
                aliases.AddRange(profile.SearchAliases);
            }

            if (Normalize(series.SeriesEdition).Equals(DirectorsCut, StringComparison.OrdinalIgnoreCase))
            {
                var baseTitle = GetBaseTitle(series.Title);

                if (baseTitle.IsNotNullOrWhiteSpace())
                {
                    aliases.Add($"{baseTitle} Director's Cut");
                    aliases.Add($"{baseTitle} Directors Cut");
                    aliases.Add($"{baseTitle} New Edit Version");
                    aliases.Add($"{baseTitle} New Edition");
                }
            }

            return aliases
                .Where(a => a.IsNotNullOrWhiteSpace())
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToList();
        }

        public static bool HasRequiredMarker(Series series, string releaseTitle)
        {
            if (series == null || releaseTitle.IsNullOrWhiteSpace())
            {
                return IsStandard(series?.SeriesEdition);
            }

            var edition = Normalize(series.SeriesEdition);

            if (edition.Equals(Standard, StringComparison.OrdinalIgnoreCase) ||
                edition.Equals(Custom, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var profile = GetEditionProfile(series);
            var marker = profile?.RequiredMarker ?? DirectorsCutMarker;

            return marker.IsMatch(releaseTitle);
        }

        public static List<Episode> ApplyEpisodeOverrides(Series series, IEnumerable<Episode> episodes)
        {
            var episodeList = episodes.ToList();
            var profile = GetEditionProfile(series);

            if (profile?.EpisodeMaps == null || !profile.EpisodeMaps.Any())
            {
                return episodeList;
            }

            return ApplyEpisodeMap(profile, episodeList);
        }

        private static List<Episode> ApplyEpisodeMap(EditionProfile profile, List<Episode> episodes)
        {
            var maps = profile.EpisodeMaps;
            var sourceEpisodes = episodes.ToDictionary(e => new EpisodeKey(e.SeasonNumber, e.EpisodeNumber));
            var mappedSeasons = maps.Select(m => m.SeasonNumber).Distinct().ToHashSet();
            var mappedEpisodes = new List<Episode>();

            foreach (var map in maps)
            {
                var sourceKey = new EpisodeKey(map.SourceSeasonNumber, map.SourceEpisodeNumber);
                var targetKey = new EpisodeKey(map.SeasonNumber, map.EpisodeNumber);

                if (!sourceEpisodes.TryGetValue(sourceKey, out var sourceEpisode) &&
                    !sourceEpisodes.TryGetValue(targetKey, out sourceEpisode))
                {
                    continue;
                }

                mappedEpisodes.Add(new Episode
                {
                    TvdbId = sourceEpisode.TvdbId,
                    SeriesId = sourceEpisode.SeriesId,
                    SeasonNumber = map.SeasonNumber,
                    EpisodeNumber = map.EpisodeNumber,
                    AbsoluteEpisodeNumber = map.EpisodeNumber,
                    Title = map.Title,
                    Overview = sourceEpisode.Overview,
                    AirDate = map.AirDate,
                    AirDateUtc = GetAirDateUtc(map.AirDate),
                    Runtime = map.Runtime,
                    FinaleType = map.FinaleType ?? sourceEpisode.FinaleType,
                    Ratings = sourceEpisode.Ratings,
                    Images = sourceEpisode.Images
                });
            }

            return episodes
                .Where(e => !mappedSeasons.Contains(e.SeasonNumber))
                .Concat(mappedEpisodes)
                .ToList();
        }

        private static EditionProfile GetEditionProfile(Series series)
        {
            if (series == null)
            {
                return null;
            }

            var edition = Normalize(series.SeriesEdition);

            return EditionProfiles.FirstOrDefault(p => p.TvdbId == series.TvdbId &&
                                                       p.SeriesEdition.Equals(edition, StringComparison.OrdinalIgnoreCase));
        }

        private static string AppendEditionSuffix(string title, string suffix)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return title;
            }

            var bracketedSuffix = $"[{suffix}]";

            if (title.EndsWith(bracketedSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return title;
            }

            return $"{title} {bracketedSuffix}";
        }

        private static string GetBaseTitle(string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return title;
            }

            return Regex.Replace(title, @"\s+\[(director'?s?\s*cut|custom)\]\s*$", string.Empty, RegexOptions.IgnoreCase);
        }

        private static DateTime? GetAirDateUtc(string airDate)
        {
            if (airDate.IsNullOrWhiteSpace())
            {
                return null;
            }

            return DateTime.SpecifyKind(DateTime.Parse($"{airDate}T15:00:00"), DateTimeKind.Utc);
        }

        private static string NormalizeKnownValue(string seriesEdition)
        {
            return seriesEdition.IsNullOrWhiteSpace() ? Standard : seriesEdition.Trim().ToLowerInvariant();
        }

        private sealed class EditionProfile
        {
            public int TvdbId { get; init; }
            public string SeriesEdition { get; init; }
            public string DisplayTitle { get; init; }
            public IReadOnlyList<string> SearchAliases { get; init; } = new List<string>();
            public Regex RequiredMarker { get; init; }
            public IReadOnlyList<EditionEpisodeMap> EpisodeMaps { get; init; } = new List<EditionEpisodeMap>();
        }

        private sealed record EditionEpisodeMap(int SeasonNumber, int EpisodeNumber, int SourceSeasonNumber, int SourceEpisodeNumber, string Title, string AirDate, int Runtime, string FinaleType = null);
        private sealed record EpisodeKey(int SeasonNumber, int EpisodeNumber);
    }
}
