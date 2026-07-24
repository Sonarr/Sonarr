using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularParser : TraktParser
    {
        private readonly TraktPopularSettings _settings;

        public TraktPopularParser(TraktPopularSettings settings)
        {
            _settings = settings;
        }

        public override IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            var listItems = new List<ImportListItemInfo>();

            if (!PreProcess(importResponse))
            {
                return listItems;
            }

            var traktSeries = _settings.TraktListType switch
            {
                (int)TraktPopularListType.Popular => STJson.Deserialize<List<TraktSeriesResource>>(importResponse.Content),
                _ => STJson.Deserialize<List<TraktResponse>>(importResponse.Content).SelectList(c => c.Show)
            };

            // no series were returned
            if (traktSeries == null)
            {
                return listItems;
            }

            foreach (var series in traktSeries)
            {
                listItems.AddIfNotNull(new ImportListItemInfo
                {
                    Title = series.Title,
                    TvdbId = series.Ids.Tvdb.GetValueOrDefault(),
                });
            }

            return listItems;
        }
    }
}
