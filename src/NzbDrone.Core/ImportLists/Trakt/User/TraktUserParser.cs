using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserParser : TraktParser
    {
        private readonly TraktUserSettings _settings;
        private ImportListResponse _importResponse;

        public TraktUserParser(TraktUserSettings settings)
        {
            _settings = settings;
        }

        public override IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var listItems = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return listItems;
            }

            var traktSeries = new List<TraktSeriesResource>();

            if (_settings.TraktListType == (int)TraktUserListType.UserWatchedList)
            {
                var jsonWatchedResponse = STJson.Deserialize<List<TraktWatchedResponse>>(_importResponse.Content);

                switch (_settings.TraktWatchedListType)
                {
                    case (int)TraktUserWatchedListType.InProgress:
                        traktSeries = jsonWatchedResponse.Where(c => c.Seasons.Where(s => s.Number > 0).Sum(s => s.Episodes.Count) < c.Show.AiredEpisodes).SelectList(c => c.Show);
                        break;
                    case (int)TraktUserWatchedListType.CompletelyWatched:
                        traktSeries = jsonWatchedResponse.Where(c => c.Seasons.Where(s => s.Number > 0).Sum(s => s.Episodes.Count) == c.Show.AiredEpisodes).SelectList(c => c.Show);
                        break;
                    default:
                        traktSeries = jsonWatchedResponse.SelectList(c => c.Show);
                        break;
                }
            }
            else
            {
                traktSeries = STJson.Deserialize<List<TraktResponse>>(_importResponse.Content).SelectList(c => c.Show);
            }

            // no series were returned
            if (traktSeries == null)
            {
                return listItems;
            }

            foreach (var series in traktSeries)
            {
                listItems.AddIfNotNull(new ImportListItemInfo()
                {
                    Title = series.Title,
                    TvdbId = series.Ids.Tvdb.GetValueOrDefault(),
                });
            }

            return listItems;
        }
    }
}
