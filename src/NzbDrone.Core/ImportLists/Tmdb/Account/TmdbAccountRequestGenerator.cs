using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountRequestGenerator : TmdbRequestGeneratorBase<TmdbAccountSettings>
{
    public TmdbAccountRequestGenerator(TmdbAccountSettings settings, int maxPages)
        : base(settings, maxPages)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
    {
        builder.ResourceUrl = (TmdbAccountListType)Settings.AccountListType switch
        {
            TmdbAccountListType.Rated => $"4/account/{Settings.AccountId}/tv/rated",
            TmdbAccountListType.Recommended => $"4/account/{Settings.AccountId}/tv/recommendations",
            TmdbAccountListType.Watchlist => $"4/account/{Settings.AccountId}/tv/watchlist",
            _ => $"4/account/{Settings.AccountId}/tv/favorites"
        };
    }
}
