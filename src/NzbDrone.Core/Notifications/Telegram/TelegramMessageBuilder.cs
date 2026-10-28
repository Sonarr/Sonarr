using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Telegram
{
    internal static class TelegramMessageBuilder
    {
        // Telegram counts parsed text, so markup is free. Captions on a photo are
        // capped far lower than a plain message, which is what forces truncation.
        private const int MaxCaptionLength = 1024;
        private const int MaxTextLength = 4096;

        // An overview cut below this is more distracting than no overview at all.
        private const int MinOverviewLength = 60;

        public static TelegramMessage BuildSimpleMessage(string title, string message, IEnumerable<NotificationMetadataLink> links)
        {
            var text = new StringBuilder($"<b>{HttpUtility.HtmlEncode(title)}</b>\n");

            text.AppendLine(HttpUtility.HtmlEncode(message));

            foreach (var link in links)
            {
                text.Append("<a href=\"").Append(link.Link).Append("\">")
                    .Append(HttpUtility.HtmlEncode(link.Label)).AppendLine("</a>");
            }

            return new TelegramMessage { Text = text.ToString() };
        }

        public static TelegramMessage BuildEpisodeMessage(string trigger, Series series, IReadOnlyList<Episode> episodes, QualityModel quality, long? size, IReadOnlyCollection<NotificationMetadataLink> links, TelegramSettings settings)
        {
            var first = episodes[0];

            var card = new Card
            {
                Heading = $"{series.Title} · {GetEpisodeIdentifier(series, episodes)}",
                Subheading = episodes.Select(e => e.Title).Where(t => t.IsNotNullOrWhiteSpace()).Join(" + "),
                Overview = first.Overview,
                Facts = JoinFacts(FormatAirDate(first), FormatRuntime(first)),
                FileFacts = JoinFacts(FormatQuality(series, quality), size > 0 ? size.Value.SizeSuffix() : null),
                Trigger = trigger,
                Links = links
            };

            return Render(card, SelectImage(series, episodes, (TelegramImageType)settings.Image));
        }

        public static TelegramMessage BuildSeriesMessage(string trigger, Series series, IReadOnlyCollection<NotificationMetadataLink> links, TelegramSettings settings)
        {
            var card = new Card
            {
                Heading = series.Year > 0 ? $"{series.Title} ({series.Year})" : series.Title,
                Overview = series.Overview,
                Facts = JoinFacts(FormatRating(series.Ratings), series.Network),
                Trigger = trigger,
                Links = links
            };

            return Render(card, SelectImage(series, null, (TelegramImageType)settings.Image));
        }

        private static TelegramMessage Render(Card card, string photoUrl)
        {
            // Measure the markup-free rendering, since that is what Telegram counts,
            // then spend whatever is left on the overview. Adding it also adds a blank
            // line and its own line break, so two characters go with it.
            var limit = photoUrl.IsNotNullOrWhiteSpace() ? MaxCaptionLength : MaxTextLength;
            var overview = Truncate(card.Overview, limit - card.Render(string.Empty, false).Length - 2);

            return new TelegramMessage
            {
                Text = card.Render(overview, true),
                Photo = photoUrl
            };
        }

        private static string Truncate(string text, int available)
        {
            if (text.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            if (text.Length <= available)
            {
                return text;
            }

            if (available < MinOverviewLength)
            {
                return string.Empty;
            }

            // Leave room for the ellipsis and break on a word boundary.
            var cut = text.LastIndexOf(' ', available - 1);

            if (cut < MinOverviewLength)
            {
                cut = available - 1;
            }

            return text[..cut].TrimEnd(' ', ',', ';', ':', '.', '-') + "…";
        }

        private static string GetEpisodeIdentifier(Series series, IReadOnlyList<Episode> episodes)
        {
            var first = episodes[0];

            if (series.SeriesType == SeriesTypes.Daily && first.AirDate.IsNotNullOrWhiteSpace())
            {
                return first.AirDate;
            }

            var numbers = episodes.Select(e => $"E{e.EpisodeNumber:00}").Join("-");

            return $"S{first.SeasonNumber:00}{numbers}";
        }

        private static string FormatRating(Ratings ratings)
        {
            if (ratings == null || ratings.Votes <= 0 || ratings.Value <= 0)
            {
                return null;
            }

            return ratings.Value.ToString("0.0", CultureInfo.InvariantCulture) + "/10";
        }

        private static string FormatAirDate(Episode episode)
        {
            if (episode.AirDate.IsNotNullOrWhiteSpace())
            {
                return episode.AirDate;
            }

            return episode.AirDateUtc?.ToString(Episode.AIR_DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        private static string FormatRuntime(Episode episode)
        {
            return episode.Runtime > 0 ? $"{episode.Runtime} min" : null;
        }

        private static string FormatQuality(Series series, QualityModel quality)
        {
            if (quality?.Quality == null)
            {
                return null;
            }

            var qualityString = quality.Quality.ToString();

            if (quality.Revision.Version > 1)
            {
                qualityString += series.SeriesType == SeriesTypes.Anime ? " v" + quality.Revision.Version : " Proper";
            }

            return qualityString;
        }

        private static string JoinFacts(params string[] facts)
        {
            return facts.Where(f => f.IsNotNullOrWhiteSpace()).Join(" · ");
        }

        private static string SelectImage(Series series, IReadOnlyList<Episode> episodes, TelegramImageType preferred)
        {
            if (preferred == TelegramImageType.None)
            {
                return null;
            }

            // Fall through the remaining types so a missing screenshot - common for
            // episodes downloaded the day they air - still produces an image.
            var order = new[] { preferred }
                .Concat(new[] { TelegramImageType.Screenshot, TelegramImageType.Fanart, TelegramImageType.Poster }
                    .Where(t => t != preferred));

            foreach (var type in order)
            {
                var url = type switch
                {
                    TelegramImageType.Screenshot => episodes?
                        .SelectMany(e => e.Images ?? [])
                        .FirstOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot)?.RemoteUrl,
                    TelegramImageType.Fanart => FindSeriesImage(series, MediaCoverTypes.Fanart),
                    TelegramImageType.Poster => FindSeriesImage(series, MediaCoverTypes.Poster),
                    _ => null
                };

                if (url.IsNotNullOrWhiteSpace())
                {
                    return url;
                }
            }

            return null;
        }

        private static string FindSeriesImage(Series series, MediaCoverTypes coverType)
        {
            return series?.Images?.FirstOrDefault(i => i.CoverType == coverType)?.RemoteUrl;
        }

        // Telegram only requires these three escaped. HtmlEncode would also turn
        // characters like the separator into numeric entities for no benefit.
        private static string Escape(string value)
        {
            return value?.Replace("&", "&amp;", StringComparison.Ordinal)
                         .Replace("<", "&lt;", StringComparison.Ordinal)
                         .Replace(">", "&gt;", StringComparison.Ordinal);
        }

        private sealed class Card
        {
            public string Heading { get; init; }
            public string Subheading { get; init; }
            public string Overview { get; init; }
            public string Facts { get; init; }
            public string FileFacts { get; init; }
            public string Trigger { get; init; }
            public IReadOnlyCollection<NotificationMetadataLink> Links { get; init; }

            public string Render(string overview, bool markup)
            {
                string Encode(string value) => markup ? Escape(value) : value;
                string Bold(string value) => markup ? $"<b>{Encode(value)}</b>" : value;
                string Italic(string value) => markup ? $"<i>{Encode(value)}</i>" : value;

                List<string> lines = [Bold(Heading)];

                if (Subheading.IsNotNullOrWhiteSpace())
                {
                    lines.Add(Italic(Subheading));
                }

                if (overview.IsNotNullOrWhiteSpace())
                {
                    lines.Add(string.Empty);
                    lines.Add(markup ? $"<blockquote expandable>{Encode(overview)}</blockquote>" : overview);
                }

                lines.Add(string.Empty);

                if (Facts.IsNotNullOrWhiteSpace())
                {
                    lines.Add(Encode(Facts));
                }

                if (FileFacts.IsNotNullOrWhiteSpace())
                {
                    lines.Add(Italic(FileFacts));
                }

                lines.Add(Italic($"Trigger: {Trigger}"));

                if (Links != null && Links.Any())
                {
                    lines.Add(markup
                        ? Links.Select(l => $"<a href=\"{l.Link}\">{Escape(l.Label)}</a>").Join(" · ")
                        : Links.Select(l => l.Label).Join(" · "));
                }

                return lines.Join("\n");
            }
        }
    }
}
