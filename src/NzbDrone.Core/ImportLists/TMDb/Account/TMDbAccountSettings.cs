using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.TMDb.Account;

public sealed class TMDbAccountSettingsValidator : TMDbSettingsBaseValidator<TMDbAccountSettings>
{
}

public sealed class TMDbAccountSettings : TMDbSettingsBase<TMDbAccountSettings>
{
    private static readonly TMDbAccountSettingsValidator Validator = new();

    public TMDbAccountSettings()
        : base(Validator)
    {
        AccountList = (int)TMDbAccountList.Watchlist;
    }

    [FieldDefinition(1, Label = "Account List Type", HelpText = "Select the list type of the account to import.", Type = FieldType.Select, SelectOptions = typeof(TMDbAccountList))]
    public int AccountList { get; set; }
}
