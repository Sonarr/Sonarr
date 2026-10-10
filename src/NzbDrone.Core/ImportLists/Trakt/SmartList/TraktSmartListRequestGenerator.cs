using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.SmartList
{
    public class TraktSmartListRequestGenerator : TraktRequestGeneratorBase<TraktSmartListSettings>
    {
        public TraktSmartListRequestGenerator(TraktSmartListSettings settings, string clientId, int pageSize, int maxNumResults)
            : base(settings, clientId, pageSize, maxNumResults)
        {
        }

        protected override void SetResource(HttpRequestBuilder requestBuilder)
        {
            requestBuilder
                .Resource("/smart-lists/{slug}/items/shows")
                .SetSegment("slug", TraktSmartListSlug.Parse(Settings.SmartList));
        }

        protected override Dictionary<string, string> GetFilterParameters()
        {
            return TraktQueryHelper.BuildFilterParameters(Settings.Rating, Settings.Genres, null, _pageSize, Settings.TraktAdditionalParameters);
        }
    }
}
