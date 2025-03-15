using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications;

public static class NotificationMetadataLinkGenerator
{
    public static List<NotificationMetadataLink> GenerateLinks(Series series, IEnumerable<int> metadataLinks)
    {
        var links = new List<NotificationMetadataLink>();

        if (series == null)
        {
            return links;
        }

        foreach (var link in metadataLinks)
        {
            var linkType = (MetadataLinkType)link;

            if (linkType == MetadataLinkType.Imdb && series.ImdbId.IsNotNullOrWhiteSpace())
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Imdb, "IMDb", $"https://www.imdb.com/title/{series.ImdbId}"));
            }

            if (linkType == MetadataLinkType.Tvdb && series.TvdbId > 0)
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Tvdb, "TVDb", $"http://www.thetvdb.com/?tab=series&id={series.TvdbId}"));
            }

            if (linkType == MetadataLinkType.Trakt && series.TvdbId > 0)
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Trakt, "Trakt", $"http://trakt.tv/search/tvdb/{series.TvdbId}?id_type=show"));
            }

            if (linkType == MetadataLinkType.Tvmaze && series.TvMazeId > 0)
            {
                links.Add(new NotificationMetadataLink(MetadataLinkType.Tvmaze, "TVMaze", $"http://www.tvmaze.com/shows/{series.TvMazeId}/_"));
            }
        }

        return links;
    }
}
