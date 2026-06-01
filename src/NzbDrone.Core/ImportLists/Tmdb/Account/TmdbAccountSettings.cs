using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountSettingsValidator : TmdbSettingsBaseValidator<TmdbAccountSettings>
{
}

public class TmdbAccountSettings : TmdbSettingsBase<TmdbAccountSettings>
{
    private static readonly TmdbAccountSettingsValidator Validator = new();

    public TmdbAccountSettings()
        : base(Validator)
    {
        AccountListType = (int)TmdbAccountListType.Watchlist;
    }

    [FieldDefinition(1, Label = "ImportListsTmdbSettingsAccountListType", HelpText = "ImportListsTmdbSettingsAccountListTypeHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbAccountListType))]
    public int AccountListType { get; set; }
}
