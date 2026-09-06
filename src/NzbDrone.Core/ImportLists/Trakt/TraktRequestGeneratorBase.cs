using System;
using System.Collections.Generic;
using System.Globalization;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public abstract class TraktRequestGeneratorBase<TSettings> : IImportListRequestGenerator
        where TSettings : TraktSettingsBase<TSettings>
    {
        protected readonly string _clientId;
        protected readonly int _pageSize;
        protected readonly int _maxNumResults;

        protected TraktRequestGeneratorBase(TSettings settings, string clientId, int pageSize, int maxNumResults)
        {
            Settings = settings;
            _clientId = clientId;
            _pageSize = pageSize;
            _maxNumResults = maxNumResults;
        }

        protected TSettings Settings { get; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        protected abstract void SetResource(HttpRequestBuilder requestBuilder);

        protected abstract Dictionary<string, string> GetFilterParameters();

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var requestBuilder = new HttpRequestBuilder(Settings.BaseUrl.Trim());

            requestBuilder
                .Accept(HttpAccept.Json)
                .SetHeader("trakt-api-version", "2")
                .SetHeader("trakt-api-key", _clientId);

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                requestBuilder.SetHeader("Authorization", $"Bearer {Settings.AccessToken}");
            }

            SetResource(requestBuilder);

            foreach (var param in GetFilterParameters())
            {
                requestBuilder.AddQueryParam(param.Key, param.Value, true);
            }

            var limit = Math.Clamp(Settings.Limit, 0, _maxNumResults);
            var maxPages = (int)Math.Ceiling(decimal.Divide(limit, _pageSize));
            var remainderLimit = limit % _pageSize;

            for (var page = 1; page <= maxPages; page++)
            {
                if (maxPages == 1 && remainderLimit > 0)
                {
                    requestBuilder.AddQueryParam("limit", remainderLimit.ToString(CultureInfo.InvariantCulture), true);
                }

                requestBuilder.AddQueryParam("page", page.ToString(CultureInfo.InvariantCulture), true);

                yield return new ImportListRequest(requestBuilder.Build());
            }
        }
    }
}
