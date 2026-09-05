using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListRequestGenerator : TraktRequestGeneratorBase<TraktListSettings>
    {
        public TraktListRequestGenerator(TraktListSettings settings, string clientId, int pageSize, int maxNumResults)
            : base(settings, clientId, pageSize, maxNumResults)
        {
        }

        protected override string Years => Settings.Years;

        protected override void SetResource(HttpRequestBuilder requestBuilder)
        {
            requestBuilder
                .Resource("/users/{userName}/lists/{listName}/items/show,season,episode")
                .SetSegment("userName", Settings.Username.Trim())
                .SetSegment("listName", Settings.Listname.ToUrlSlug());
        }
    }
}
