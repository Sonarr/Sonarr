using Workarr.Extensions;
using Workarr.Parser.Model;
using Workarr.Serializer.System.Text.Json;

namespace Workarr.ImportLists.Trakt.Popular
{
    public class TraktPopularParser : TraktParser
    {
        private readonly TraktPopularSettings _settings;
        private ImportListResponse _importResponse;

        public TraktPopularParser(TraktPopularSettings settings)
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

            if (_settings.TraktListType == (int)TraktPopularListType.Popular)
            {
                traktSeries = STJson.Deserialize<List<TraktSeriesResource>>(_importResponse.Content);
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
