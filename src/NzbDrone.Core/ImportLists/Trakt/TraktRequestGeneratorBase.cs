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
        private readonly string _clientId;
        private readonly int _pageSize;
        private readonly int _maxNumResults;

        protected TraktRequestGeneratorBase(TSettings settings, string clientId, int pageSize, int maxNumResults)
        {
            Settings = settings;
            _clientId = clientId;
            _pageSize = pageSize;
            _maxNumResults = maxNumResults;
        }

        protected TSettings Settings { get; }

        protected abstract string Years { get; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        protected abstract void SetResource(HttpRequestBuilder requestBuilder);

        protected virtual void SetFilterParameters(Dictionary<string, string> filterParams)
        {
        }

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

            var filterParams = TraktQueryHelper.BuildFilterParameters(Settings.Rating, Settings.Genres, Years, _pageSize, Settings.TraktAdditionalParameters);

            SetFilterParameters(filterParams);

            foreach (var param in filterParams)
            {
                requestBuilder.AddQueryParam(param.Key, param.Value);
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
