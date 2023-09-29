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

            var jsonResponse = new List<TraktSeriesResource>();

            if (_settings.TraktListType == (int)TraktUserListType.UserWatchedList)
            {
                var jsonWatchedResponse = STJson.Deserialize<List<TraktWatchedResponse>>(_importResponse.Content);
                switch (_settings.TraktWatchedListType)
                {
                    case (int)TraktUserWatchedListType.InProgress:
                        jsonResponse = jsonWatchedResponse.Where(c => c.Seasons.Where(s => s.Number > 0).Sum(s => s.Episodes.Count) < c.Show.AiredEpisodes).SelectList(c => c.Show);
                        break;
                    case (int)TraktUserWatchedListType.CompletelyWatched:
                        jsonResponse = jsonWatchedResponse.Where(c => c.Seasons.Where(s => s.Number > 0).Sum(s => s.Episodes.Count) == c.Show.AiredEpisodes).SelectList(c => c.Show);
                        break;
                    default:
                        jsonResponse = jsonWatchedResponse.SelectList(c => c.Show);
                        break;
                }
            }
            else
            {
                jsonResponse = STJson.Deserialize<List<TraktResponse>>(_importResponse.Content).SelectList(c => c.Show);
            }

            // no series were return
            if (jsonResponse == null)
            {
                return listItems;
            }

            foreach (var series in jsonResponse)
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
