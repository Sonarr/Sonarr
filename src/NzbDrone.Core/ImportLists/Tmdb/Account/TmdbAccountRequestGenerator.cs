using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountRequestGenerator : TmdbRequestGeneratorBase<TmdbAccountSettings>
{
    public TmdbAccountRequestGenerator(TmdbAccountSettings settings)
        : base(settings)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
    {
        builder.ResourceUrl = (TmdbAccountList)Settings.AccountList switch
        {
            TmdbAccountList.Rated => $"4/account/{Settings.AccountId}/tv/rated",
            TmdbAccountList.Recommended => $"4/account/{Settings.AccountId}/tv/recommendations",
            TmdbAccountList.Watchlist => $"4/account/{Settings.AccountId}/tv/watchlist",
            _ => $"4/account/{Settings.AccountId}/tv/favorites"
        };
    }
}
