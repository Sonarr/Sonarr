using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.Account;

public sealed class TMDbAccountRequestGenerator : TMDbRequestGeneratorBase<TMDbAccountSettings>
{
    public TMDbAccountRequestGenerator(TMDbAccountSettings settings)
        : base(settings)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
    {
        builder.ResourceUrl = (TMDbAccountList)Settings.AccountList switch
        {
            TMDbAccountList.Rated => $"4/account/{Settings.AccountId}/tv/rated",
            TMDbAccountList.Recommended => $"4/account/{Settings.AccountId}/tv/recommendations",
            TMDbAccountList.Watchlist => $"4/account/{Settings.AccountId}/tv/watchlist",
            _ => $"4/account/{Settings.AccountId}/tv/favorites"
        };
    }
}
